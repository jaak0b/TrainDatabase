using System.Globalization;
using System.IO.Ports;
using Composition;
using Core;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace SpeedSensorConsole
{
  internal class Program
  {
    public async static Task Main(string[] args)
    {
      IServiceProvider serviceProvider = Bootstrapper.Initialize(null, null);

      SpeedSensorPortFactory speedSensorPortFactory = serviceProvider.GetRequiredService<SpeedSensorPortFactory>();

      string portName = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                          .Title("Select available serial port:")
                                           // .PageSize(10)
                                          .AddChoices(SerialPort.GetPortNames()));

      int baudRate = AnsiConsole.Prompt(new TextPrompt<int>("Enter baud rate")
                                         .DefaultValue(9600));
      Console.Clear();

      using ISpeedSensorPort sensor = speedSensorPortFactory(portName, baudRate);

      using CancellationTokenSource cts = new();

      Table table = new Table().LeftAligned();
      table.AddColumn("Measure time");
      table.AddColumn("Duration (in millisecond) ");
      await AnsiConsole.Live(table)
                       .StartAsync(async ctx =>
                                   {
                                     ctx.Refresh();

                                     while (!cts.Token.IsCancellationRequested)
                                     {
                                       decimal? value = await sensor.ReadDurationAsync(TimeSpan.FromMinutes(5));

                                       table.AddRow(DateTime.Now.ToString(CultureInfo.CurrentCulture), value?.ToString() ?? "Timeout");

                                       ctx.Refresh();
                                       await Task.Delay(1000, cts.Token);
                                     }
                                   });
    }
  }
}