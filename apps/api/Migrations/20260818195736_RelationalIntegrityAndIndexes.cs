using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mooch.Api.Migrations
{
    /// <inheritdoc />
    public partial class RelationalIntegrityAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_walker_dogs_dogID",
                table: "walker_dogs");

            migrationBuilder.DropIndex(
                name: "IX_connected_accounts_UserId",
                table: "connected_accounts");

            migrationBuilder.DropIndex(
                name: "IX_activities_DogId",
                table: "activities");

            migrationBuilder.CreateIndex(
                name: "IX_walker_dogs_primary_owner",
                table: "walker_dogs",
                column: "dogID",
                unique: true,
                filter: "\"IsPrimaryOwner\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_friendships_AddresseeId_Status",
                table: "friendships",
                columns: new[] { "AddresseeId", "Status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_friendships_distinct_users",
                table: "friendships",
                sql: "\"RequesterId\" <> \"AddresseeId\"");

            migrationBuilder.CreateIndex(
                name: "IX_connected_accounts_Provider_ExternalUserId",
                table: "connected_accounts",
                columns: new[] { "Provider", "ExternalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_connected_accounts_UserId_Provider",
                table: "connected_accounts",
                columns: new[] { "UserId", "Provider" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_challenges_date_range",
                table: "challenges",
                sql: "\"EndsOn\" >= \"StartsOn\"");

            migrationBuilder.CreateIndex(
                name: "IX_challenge_participants_UserId",
                table: "challenge_participants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_activities_CreatedAtUtc",
                table: "activities",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_activities_DogId_StartedAtUtc",
                table: "activities",
                columns: new[] { "DogId", "StartedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_challenge_participants_users_UserId",
                table: "challenge_participants",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_friendships_users_AddresseeId",
                table: "friendships",
                column: "AddresseeId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_friendships_users_RequesterId",
                table: "friendships",
                column: "RequesterId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_challenge_participants_users_UserId",
                table: "challenge_participants");

            migrationBuilder.DropForeignKey(
                name: "FK_friendships_users_AddresseeId",
                table: "friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_friendships_users_RequesterId",
                table: "friendships");

            migrationBuilder.DropIndex(
                name: "IX_walker_dogs_primary_owner",
                table: "walker_dogs");

            migrationBuilder.DropIndex(
                name: "IX_friendships_AddresseeId_Status",
                table: "friendships");

            migrationBuilder.DropCheckConstraint(
                name: "CK_friendships_distinct_users",
                table: "friendships");

            migrationBuilder.DropIndex(
                name: "IX_connected_accounts_Provider_ExternalUserId",
                table: "connected_accounts");

            migrationBuilder.DropIndex(
                name: "IX_connected_accounts_UserId_Provider",
                table: "connected_accounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_challenges_date_range",
                table: "challenges");

            migrationBuilder.DropIndex(
                name: "IX_challenge_participants_UserId",
                table: "challenge_participants");

            migrationBuilder.DropIndex(
                name: "IX_activities_CreatedAtUtc",
                table: "activities");

            migrationBuilder.DropIndex(
                name: "IX_activities_DogId_StartedAtUtc",
                table: "activities");

            migrationBuilder.CreateIndex(
                name: "IX_walker_dogs_dogID",
                table: "walker_dogs",
                column: "dogID");

            migrationBuilder.CreateIndex(
                name: "IX_connected_accounts_UserId",
                table: "connected_accounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_activities_DogId",
                table: "activities",
                column: "DogId");
        }
    }
}
