using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mooch.Api.Migrations
{
    /// <inheritdoc />
    public partial class ActivityDogJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity_dogs",
                columns: table => new
                {
                    activityDogID = table.Column<Guid>(type: "uuid", nullable: false),
                    activityID = table.Column<Guid>(type: "uuid", nullable: false),
                    dogID = table.Column<Guid>(type: "uuid", nullable: false),
                    createdDateUTC = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_dogs", x => x.activityDogID);
                    table.ForeignKey(
                        name: "FK_activity_dogs_activities_activityID",
                        column: x => x.activityID,
                        principalTable: "activities",
                        principalColumn: "activityID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_activity_dogs_dogs_dogID",
                        column: x => x.dogID,
                        principalTable: "dogs",
                        principalColumn: "dogID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO activity_dogs ("activityDogID", "activityID", "dogID", "createdDateUTC")
                SELECT "activityID", "activityID", "dogID", COALESCE("createdDateUTC", NOW())
                FROM activities
                WHERE "dogID" IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_activities_dogs_dogID",
                table: "activities");

            migrationBuilder.DropIndex(
                name: "IX_activities_dogID_StartedAtUtc",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "dogID",
                table: "activities");

            migrationBuilder.CreateIndex(
                name: "IX_activity_dogs_activityID",
                table: "activity_dogs",
                column: "activityID");

            migrationBuilder.CreateIndex(
                name: "IX_activity_dogs_activityID_dogID",
                table: "activity_dogs",
                columns: new[] { "activityID", "dogID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_activity_dogs_dogID",
                table: "activity_dogs",
                column: "dogID");

            migrationBuilder.CreateIndex(
                name: "IX_activity_dogs_dogID_createdDateUTC",
                table: "activity_dogs",
                columns: new[] { "dogID", "createdDateUTC" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "dogID",
                table: "activities",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE activities AS a
                SET "dogID" = ad."dogID"
                FROM (
                    SELECT DISTINCT ON ("activityID") "activityID", "dogID"
                    FROM activity_dogs
                    ORDER BY "activityID", "createdDateUTC", "activityDogID"
                ) AS ad
                WHERE a."activityID" = ad."activityID";

                UPDATE activities
                SET "dogID" = '00000000-0000-0000-0000-000000000000'
                WHERE "dogID" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "dogID",
                table: "activities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "activity_dogs");

            migrationBuilder.CreateIndex(
                name: "IX_activities_dogID_StartedAtUtc",
                table: "activities",
                columns: new[] { "dogID", "StartedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_activities_dogs_dogID",
                table: "activities",
                column: "dogID",
                principalTable: "dogs",
                principalColumn: "dogID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
