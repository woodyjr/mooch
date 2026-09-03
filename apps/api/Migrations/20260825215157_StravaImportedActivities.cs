using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mooch.Api.Migrations
{
    /// <inheritdoc />
    public partial class StravaImportedActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "imported_activities",
                columns: table => new
                {
                    importedActivityID = table.Column<Guid>(type: "uuid", nullable: false),
                    walkerID = table.Column<Guid>(type: "uuid", nullable: false),
                    connectedAccountID = table.Column<Guid>(type: "uuid", nullable: false),
                    externalActivityID = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    activityType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    DistanceMiles = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    requiresDogAssignment = table.Column<bool>(type: "boolean", nullable: false),
                    createdDateUTC = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imported_activities", x => x.importedActivityID);
                    table.ForeignKey(
                        name: "FK_imported_activities_connected_accounts_connectedAccountID",
                        column: x => x.connectedAccountID,
                        principalTable: "connected_accounts",
                        principalColumn: "connectedAccountID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_imported_activities_walkers_walkerID",
                        column: x => x.walkerID,
                        principalTable: "walkers",
                        principalColumn: "walkerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_imported_activities_connectedAccountID_externalActivityID",
                table: "imported_activities",
                columns: new[] { "connectedAccountID", "externalActivityID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_imported_activities_walkerID_StartedAtUtc",
                table: "imported_activities",
                columns: new[] { "walkerID", "StartedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "imported_activities");
        }
    }
}
