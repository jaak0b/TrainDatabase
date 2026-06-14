using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainDatabase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MultiTractionJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleTractions",
                columns: table => new
                {
                    LeadVehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberVehicleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTractions", x => new { x.LeadVehicleId, x.MemberVehicleId });
                    table.ForeignKey(
                        name: "FK_VehicleTractions_Vehicles_LeadVehicleId",
                        column: x => x.LeadVehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleTractions_Vehicles_MemberVehicleId",
                        column: x => x.MemberVehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTractions_MemberVehicleId",
                table: "VehicleTractions",
                column: "MemberVehicleId");

            migrationBuilder.Sql(@"
                INSERT INTO VehicleTractions (LeadVehicleId, MemberVehicleId)
                WITH RECURSIVE split(LeadId, token, rest) AS (
                    SELECT Id, '', TractionVehicleIds || ';'
                    FROM Vehicles
                    WHERE TractionVehicleIds IS NOT NULL AND TractionVehicleIds <> ''
                    UNION ALL
                    SELECT LeadId,
                           substr(rest, 1, instr(rest, ';') - 1),
                           substr(rest, instr(rest, ';') + 1)
                    FROM split
                    WHERE rest <> ''
                )
                SELECT DISTINCT LeadId, CAST(token AS INTEGER)
                FROM split
                WHERE token <> ''
                  AND CAST(token AS INTEGER) <> LeadId
                  AND CAST(token AS INTEGER) IN (SELECT Id FROM Vehicles);");

            migrationBuilder.DropColumn(
                name: "TractionVehicleIds",
                table: "Vehicles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TractionVehicleIds",
                table: "Vehicles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE Vehicles
                SET TractionVehicleIds = COALESCE(
                    (SELECT group_concat(MemberVehicleId, ';') FROM VehicleTractions WHERE LeadVehicleId = Vehicles.Id), '');");

            migrationBuilder.DropTable(
                name: "VehicleTractions");
        }
    }
}
