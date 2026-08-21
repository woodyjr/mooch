using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mooch.Api.Migrations
{
    /// <inheritdoc />
    public partial class DogOwnerInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dog_owner_invites",
                columns: table => new
                {
                    dogOwnerInviteID = table.Column<Guid>(type: "uuid", nullable: false),
                    dogID = table.Column<Guid>(type: "uuid", nullable: false),
                    invitedByWalkerID = table.Column<Guid>(type: "uuid", nullable: false),
                    inviteeEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    createdDateUTC = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    respondedDateUTC = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dog_owner_invites", x => x.dogOwnerInviteID);
                    table.ForeignKey(
                        name: "FK_dog_owner_invites_dogs_dogID",
                        column: x => x.dogID,
                        principalTable: "dogs",
                        principalColumn: "dogID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dog_owner_invites_walkers_invitedByWalkerID",
                        column: x => x.invitedByWalkerID,
                        principalTable: "walkers",
                        principalColumn: "walkerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dog_owner_invites_dogID",
                table: "dog_owner_invites",
                column: "dogID");

            migrationBuilder.CreateIndex(
                name: "IX_dog_owner_invites_invitedByWalkerID",
                table: "dog_owner_invites",
                column: "invitedByWalkerID");

            migrationBuilder.CreateIndex(
                name: "IX_dog_owner_invites_pending_email",
                table: "dog_owner_invites",
                columns: new[] { "dogID", "inviteeEmail", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dog_owner_invites");
        }
    }
}
