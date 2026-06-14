using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainDatabase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ImageName = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxSpeed = table.Column<long>(type: "INTEGER", nullable: true),
                    RegulationStep = table.Column<int>(type: "INTEGER", nullable: false),
                    Address = table.Column<long>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Position = table.Column<long>(type: "INTEGER", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Railway = table.Column<string>(type: "TEXT", nullable: false),
                    InvertTraction = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Dummy = table.Column<bool>(type: "INTEGER", nullable: true),
                    TractionForward = table.Column<string>(type: "TEXT", nullable: false),
                    TractionBackward = table.Column<string>(type: "TEXT", nullable: false),
                    TractionVehicleIds = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Functions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    ButtonType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageName = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<int>(type: "INTEGER", nullable: false),
                    ShowFunctionNumber = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsConfigured = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnumType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Functions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Functions_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleCalibrationData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Direction = table.Column<bool>(type: "INTEGER", nullable: false),
                    SpeedStep = table.Column<int>(type: "INTEGER", nullable: false),
                    MeasuredSpeed = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleCalibrationData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleCalibrationData_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Functions_VehicleId",
                table: "Functions",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleCalibrationData_VehicleId_Direction_SpeedStep",
                table: "VehicleCalibrationData",
                columns: new[] { "VehicleId", "Direction", "SpeedStep" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Functions");

            migrationBuilder.DropTable(
                name: "VehicleCalibrationData");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
