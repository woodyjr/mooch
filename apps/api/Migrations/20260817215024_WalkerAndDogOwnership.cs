using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mooch.Api.Migrations
{
    /// <inheritdoc />
    public partial class WalkerAndDogOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activities_dogs_DogId",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_dogs_users_OwnerId",
                table: "dogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dogs",
                table: "dogs");

            migrationBuilder.DropIndex(
                name: "IX_dogs_OwnerId",
                table: "dogs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "dogs");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "dogs",
                newName: "dogID");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "dogs",
                newName: "createdDateUTC");

            migrationBuilder.AddColumn<string>(
                name: "AvatarImage",
                table: "users",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarImage",
                table: "dogs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_dogs",
                table: "dogs",
                column: "dogID");

            migrationBuilder.CreateTable(
                name: "walkers",
                columns: table => new
                {
                    walkerID = table.Column<Guid>(type: "uuid", nullable: false),
                    userID = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AvatarImage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    createdDateUTC = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_walkers", x => x.walkerID);
                    table.ForeignKey(
                        name: "FK_walkers_users_userID",
                        column: x => x.userID,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "walker_dogs",
                columns: table => new
                {
                    walkerDogID = table.Column<Guid>(type: "uuid", nullable: false),
                    walkerID = table.Column<Guid>(type: "uuid", nullable: false),
                    dogID = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimaryOwner = table.Column<bool>(type: "boolean", nullable: false),
                    createdDateUTC = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_walker_dogs", x => x.walkerDogID);
                    table.ForeignKey(
                        name: "FK_walker_dogs_dogs_dogID",
                        column: x => x.dogID,
                        principalTable: "dogs",
                        principalColumn: "dogID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_walker_dogs_walkers_walkerID",
                        column: x => x.walkerID,
                        principalTable: "walkers",
                        principalColumn: "walkerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_walker_dogs_dogID",
                table: "walker_dogs",
                column: "dogID");

            migrationBuilder.CreateIndex(
                name: "IX_walker_dogs_walkerID_dogID",
                table: "walker_dogs",
                columns: new[] { "walkerID", "dogID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_walkers_userID",
                table: "walkers",
                column: "userID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_activities_dogs_DogId",
                table: "activities",
                column: "DogId",
                principalTable: "dogs",
                principalColumn: "dogID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activities_dogs_DogId",
                table: "activities");

            migrationBuilder.DropTable(
                name: "walker_dogs");

            migrationBuilder.DropTable(
                name: "walkers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dogs",
                table: "dogs");

            migrationBuilder.DropColumn(
                name: "AvatarImage",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AvatarImage",
                table: "dogs");

            migrationBuilder.RenameColumn(
                name: "createdDateUTC",
                table: "dogs",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "dogID",
                table: "dogs",
                newName: "OwnerId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "dogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_dogs",
                table: "dogs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_dogs_OwnerId",
                table: "dogs",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_activities_dogs_DogId",
                table: "activities",
                column: "DogId",
                principalTable: "dogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dogs_users_OwnerId",
                table: "dogs",
                column: "OwnerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
