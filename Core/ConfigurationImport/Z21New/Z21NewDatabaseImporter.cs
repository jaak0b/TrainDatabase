using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ConfigurationImport.Z21New.TDO;
using Dapper;
using Helper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Persistence.Database;
using Persistence.Entities;
using Persistence.Enums;
using Persistence.Extensions;
using Persistence.Model;
using Serilog;

namespace Core.ConfigurationImport.Z21New
{
  public class Z21NewDatabaseImporter(Database database) : IDatabaseImporter
  {

    private Database Database { get; set; } = database;

    /// <summary>
    /// Import the Data from a z21 file into the internal database.
    /// </summary>
    /// <param name="z21File"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task ImportAsync(FileInfo z21File)
    {
      await Task.Run(async () =>
                     {
                       if (z21File.Extension.ToLower() is not ".z21")
                       {
                         throw new NotSupportedException($"Importing a {z21File.Extension} file is not supported!");
                       }

                       Database.DeleteAll();

                       string tempPath = Configuration.ApplicationData.TempPath.FullName;
                       FileInfo filePath = await ExtractPhotosAndSqlFileFromZ21File(z21File.FullName, tempPath);
                       await using SqliteConnection con = new($"Data Source={filePath}");
                       await MapVehiclesAsync(con);
                       await MapFunctionsAsync(con, true);
                       await con.CloseAsync();
                     });
    }

    private async Task MapFunctionsAsync(SqliteConnection con, bool removeEmptyFunctions)
    {
      List<FunctionDTO> f = con.Query<FunctionDTO>("Select * from functions").ToList();
      List<VehicleFunctionEntity> functions = f.Select(functionDto => new VehicleFunctionEntity
                                                                      {
                                                                        Id = (int)functionDto.id,
                                                                        Vehicle = Database.Vehicles.FirstOrDefault(v => v.Id == (int)functionDto.vehicle_id),
                                                                        ButtonType = (ButtonType)(int)functionDto.button_type,
                                                                        Name = functionDto.shortcut.IsNullOrWhiteSpace() ? functionDto.image_name : functionDto.shortcut,
                                                                        Time = (int)decimal.Parse(functionDto.time),
                                                                        Position = (int)functionDto.position,
                                                                        ImageName = functionDto.image_name,
                                                                        Address = (int)functionDto.function,
                                                                        ShowFunctionNumber = functionDto.show_function_number == 1,
                                                                        IsConfigured = functionDto.is_configured == 1
                                                                      })
                                               .Where(functionModel => functionModel.Vehicle is not null)
                                               .ToList();

      functions = functions.Where(e => removeEmptyFunctions && e.Name is not "Empty").ToList();

      await Database.AddRangeAsync(functions);
      await Database.SaveChangesAsync();
      Database.InvokeCollectionChanged();
    }

    private async Task MapVehiclesAsync(SqliteConnection con)
    {
      List<VehicleDTO> v = con.Query<VehicleDTO>("Select * from vehicles").ToList();
      List<VehicleEntity> vehicles = v.Select(vehicleDto => new VehicleEntity
                                                            {
                                                              Id = (int)vehicleDto.id,
                                                              Name = vehicleDto.name,
                                                              ImageName = vehicleDto.image_name,
                                                              Type = (VehicleType)(int)vehicleDto.type,
                                                              MaxSpeed = vehicleDto.max_speed,
                                                              RegulationStep = GetRegulationStep(vehicleDto.speed_display),
                                                              Address = vehicleDto.address,
                                                              IsActive = vehicleDto.active == 1,
                                                              Position = vehicleDto.position,
                                                              FullName = vehicleDto.full_name,
                                                              Railway = vehicleDto.railway,
                                                              InvertTraction = vehicleDto.traction_direction == 1,
                                                              Description = vehicleDto.description,
                                                              Dummy = vehicleDto.dummy == 1
                                                            })
                                      .ToList();
      await Database.AddRangeAsync(vehicles);
      await Database.SaveChangesAsync();
      await Database.Vehicles.Where(vehicleModel => string.IsNullOrWhiteSpace(vehicleModel.Name)).ForEachAsync(model => Log.Warning($"Imported Vehicle with Adresse {model.Address} has no display name!"));
    }

    private static RegulationStep GetRegulationStep(long speedStep)
    {
      return speedStep switch
             {
               14 => RegulationStep.Step14,
               28 => RegulationStep.Step28,
               128 => RegulationStep.Step128,
               _ => RegulationStep.Step128 // If we don't know the value we assume its 128.
             };
    }

    /// <summary>
    /// Extracts the database file and pictures from the Roco .z21 archive file.
    /// </summary>
    /// <returns>Returns the location of the sql database file.</returns>
    private async Task<FileInfo> ExtractPhotosAndSqlFileFromZ21File(string z21Path, string tempPath)
    {
      return await Task.Run(() =>
                            {
                              if (Directory.Exists(tempPath))
                              {
                                Directory.Delete(tempPath, true);
                              }

                              Directory.CreateDirectory(tempPath);


                              if (Directory.Exists(Configuration.ApplicationData.VehicleImages.FullName))
                              {
                                Directory.Delete(Configuration.ApplicationData.VehicleImages.FullName, true);
                              }

                              Directory.CreateDirectory(Configuration.ApplicationData.VehicleImages.FullName);

                              //Copy the z21File to the temp location
                              string z21PathNew = Path.Combine(tempPath, Path.GetFileName(z21Path));
                              File.Copy(z21Path, z21PathNew);
                              z21Path = z21PathNew;

                              //Rename the .z21 file to .zip.
                              string zipFileLocation = new StringBuilder(z21Path).Replace(".z21", ".zip").ToString();
                              File.Copy(z21Path, zipFileLocation);

                              //Extract the zip file and delte the zip and z21 file.
                              ZipFile.ExtractToDirectory(zipFileLocation, tempPath);
                              File.Delete(zipFileLocation);
                              File.Delete(z21Path);

                              string firstLayer = Directory.GetDirectories(tempPath).FirstOrDefault() ?? throw new InvalidOperationException(tempPath);

                              string secondLayer = Directory.GetDirectories(firstLayer).FirstOrDefault() ?? throw new InvalidOperationException(firstLayer);

                              List<string> files = Directory.GetFiles(secondLayer).ToList();

                              string sqlLiteDB = files.FirstOrDefault(e => Path.GetExtension(e).ToLower() == ".sqlite") ?? throw new FileNotFoundException("Failed to find the required .sql file!");

                              foreach (string image in files.Where(e => Path.GetExtension(e) != ".sqlite").ToList())
                              {
                                File.Move(image, $"{Configuration.ApplicationData.VehicleImages.FullName}\\{Path.GetFileName(image)}");
                              }

                              return new FileInfo(sqlLiteDB);
                            });
    }
  }
}