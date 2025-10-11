using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegulationStepToVehicleEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Speedstep",
                table: "Vehicles",
                newName: "RegulationStep");
            migrationBuilder.Sql("UPDATE Vehicles SET RegulationStep = 128");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegulationStep",
                table: "Vehicles",
                newName: "Speedstep");
        }
    }
}
