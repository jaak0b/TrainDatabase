using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleCalibrationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_VehicleCalibrationData_VehicleId_Direction_SpeedStep",
                table: "VehicleCalibrationData",
                columns: new[] { "VehicleId", "Direction", "SpeedStep" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleCalibrationData");
        }
    }
}
