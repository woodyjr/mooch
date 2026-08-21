using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mooch.Api.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeIdentifiersAndActivityOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activities_dogs_DogId",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_challenge_participants_challenges_ChallengeId",
                table: "challenge_participants");

            migrationBuilder.DropForeignKey(
                name: "FK_challenge_participants_users_UserId",
                table: "challenge_participants");

            migrationBuilder.DropForeignKey(
                name: "FK_connected_accounts_users_UserId",
                table: "connected_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_friendships_users_AddresseeId",
                table: "friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_friendships_users_RequesterId",
                table: "friendships");

            migrationBuilder.DropCheckConstraint(
                name: "CK_friendships_distinct_users",
                table: "friendships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_activities",
                table: "activities");

            migrationBuilder.RenameColumn(
                name: "RequesterId",
                table: "friendships",
                newName: "requesterUserID");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "friendships",
                newName: "createdDateUTC");

            migrationBuilder.RenameColumn(
                name: "AddresseeId",
                table: "friendships",
                newName: "addresseeUserID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "friendships",
                newName: "friendshipID");

            migrationBuilder.RenameIndex(
                name: "IX_friendships_RequesterId_AddresseeId",
                table: "friendships",
                newName: "IX_friendships_requesterUserID_addresseeUserID");

            migrationBuilder.RenameIndex(
                name: "IX_friendships_AddresseeId_Status",
                table: "friendships",
                newName: "IX_friendships_addresseeUserID_Status");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "connected_accounts",
                newName: "userID");

            migrationBuilder.RenameColumn(
                name: "ExternalUserId",
                table: "connected_accounts",
                newName: "externalUserID");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "connected_accounts",
                newName: "createdDateUTC");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "connected_accounts",
                newName: "connectedAccountID");

            migrationBuilder.RenameIndex(
                name: "IX_connected_accounts_UserId_Provider",
                table: "connected_accounts",
                newName: "IX_connected_accounts_userID_Provider");

            migrationBuilder.RenameIndex(
                name: "IX_connected_accounts_Provider_ExternalUserId",
                table: "connected_accounts",
                newName: "IX_connected_accounts_Provider_externalUserID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "challenges",
                newName: "challengeID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "challenge_participants",
                newName: "userID");

            migrationBuilder.RenameColumn(
                name: "ChallengeId",
                table: "challenge_participants",
                newName: "challengeID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "challenge_participants",
                newName: "challengeParticipantID");

            migrationBuilder.RenameIndex(
                name: "IX_challenge_participants_UserId",
                table: "challenge_participants",
                newName: "IX_challenge_participants_userID");

            migrationBuilder.RenameIndex(
                name: "IX_challenge_participants_ChallengeId_UserId",
                table: "challenge_participants",
                newName: "IX_challenge_participants_challengeID_userID");

            migrationBuilder.RenameColumn(
                name: "DogId",
                table: "activities",
                newName: "dogID");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "activities",
                newName: "createdDateUTC");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "activities",
                newName: "activityID");

            migrationBuilder.RenameIndex(
                name: "IX_activities_DogId_StartedAtUtc",
                table: "activities",
                newName: "IX_activities_dogID_StartedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_activities_CreatedAtUtc",
                table: "activities",
                newName: "IX_activities_createdDateUTC");

            migrationBuilder.AddColumn<Guid>(
                name: "walkerID",
                table: "activities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "connectedAccountID",
                table: "activities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "externalActivityID",
                table: "activities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE activities AS activity
                SET "walkerID" = ownership."walkerID"
                FROM (
                    SELECT DISTINCT ON ("dogID")
                        "dogID",
                        "walkerID"
                    FROM walker_dogs
                    ORDER BY "dogID", "IsPrimaryOwner" DESC, "createdDateUTC"
                ) AS ownership
                WHERE ownership."dogID" = activity."dogID";
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "walkerID",
                table: "activities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_activities",
                table: "activities",
                column: "activityID");

            migrationBuilder.AddCheckConstraint(
                name: "CK_friendships_distinct_users",
                table: "friendships",
                sql: "\"requesterUserID\" <> \"addresseeUserID\"");

            migrationBuilder.CreateIndex(
                name: "IX_activities_connectedAccountID_externalActivityID",
                table: "activities",
                columns: new[] { "connectedAccountID", "externalActivityID" },
                unique: true,
                filter: "\"connectedAccountID\" IS NOT NULL AND \"externalActivityID\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_activities_walkerID_StartedAtUtc",
                table: "activities",
                columns: new[] { "walkerID", "StartedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_activities_connected_accounts_connectedAccountID",
                table: "activities",
                column: "connectedAccountID",
                principalTable: "connected_accounts",
                principalColumn: "connectedAccountID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_activities_dogs_dogID",
                table: "activities",
                column: "dogID",
                principalTable: "dogs",
                principalColumn: "dogID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activities_walkers_walkerID",
                table: "activities",
                column: "walkerID",
                principalTable: "walkers",
                principalColumn: "walkerID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_challenge_participants_challenges_challengeID",
                table: "challenge_participants",
                column: "challengeID",
                principalTable: "challenges",
                principalColumn: "challengeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_challenge_participants_users_userID",
                table: "challenge_participants",
                column: "userID",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_connected_accounts_users_userID",
                table: "connected_accounts",
                column: "userID",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_friendships_users_addresseeUserID",
                table: "friendships",
                column: "addresseeUserID",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_friendships_users_requesterUserID",
                table: "friendships",
                column: "requesterUserID",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activities_connected_accounts_connectedAccountID",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_activities_dogs_dogID",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_activities_walkers_walkerID",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_challenge_participants_challenges_challengeID",
                table: "challenge_participants");

            migrationBuilder.DropForeignKey(
                name: "FK_challenge_participants_users_userID",
                table: "challenge_participants");

            migrationBuilder.DropForeignKey(
                name: "FK_connected_accounts_users_userID",
                table: "connected_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_friendships_users_addresseeUserID",
                table: "friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_friendships_users_requesterUserID",
                table: "friendships");

            migrationBuilder.DropCheckConstraint(
                name: "CK_friendships_distinct_users",
                table: "friendships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_activities",
                table: "activities");

            migrationBuilder.DropIndex(
                name: "IX_activities_connectedAccountID_externalActivityID",
                table: "activities");

            migrationBuilder.DropIndex(
                name: "IX_activities_walkerID_StartedAtUtc",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "walkerID",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "connectedAccountID",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "externalActivityID",
                table: "activities");

            migrationBuilder.RenameColumn(
                name: "requesterUserID",
                table: "friendships",
                newName: "RequesterId");

            migrationBuilder.RenameColumn(
                name: "createdDateUTC",
                table: "friendships",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "addresseeUserID",
                table: "friendships",
                newName: "AddresseeId");

            migrationBuilder.RenameColumn(
                name: "friendshipID",
                table: "friendships",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_friendships_requesterUserID_addresseeUserID",
                table: "friendships",
                newName: "IX_friendships_RequesterId_AddresseeId");

            migrationBuilder.RenameIndex(
                name: "IX_friendships_addresseeUserID_Status",
                table: "friendships",
                newName: "IX_friendships_AddresseeId_Status");

            migrationBuilder.RenameColumn(
                name: "userID",
                table: "connected_accounts",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "externalUserID",
                table: "connected_accounts",
                newName: "ExternalUserId");

            migrationBuilder.RenameColumn(
                name: "createdDateUTC",
                table: "connected_accounts",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "connectedAccountID",
                table: "connected_accounts",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_connected_accounts_userID_Provider",
                table: "connected_accounts",
                newName: "IX_connected_accounts_UserId_Provider");

            migrationBuilder.RenameIndex(
                name: "IX_connected_accounts_Provider_externalUserID",
                table: "connected_accounts",
                newName: "IX_connected_accounts_Provider_ExternalUserId");

            migrationBuilder.RenameColumn(
                name: "challengeID",
                table: "challenges",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "userID",
                table: "challenge_participants",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "challengeID",
                table: "challenge_participants",
                newName: "ChallengeId");

            migrationBuilder.RenameColumn(
                name: "challengeParticipantID",
                table: "challenge_participants",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_challenge_participants_userID",
                table: "challenge_participants",
                newName: "IX_challenge_participants_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_challenge_participants_challengeID_userID",
                table: "challenge_participants",
                newName: "IX_challenge_participants_ChallengeId_UserId");

            migrationBuilder.RenameColumn(
                name: "dogID",
                table: "activities",
                newName: "DogId");

            migrationBuilder.RenameColumn(
                name: "activityID",
                table: "activities",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "createdDateUTC",
                table: "activities",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_activities_dogID_StartedAtUtc",
                table: "activities",
                newName: "IX_activities_DogId_StartedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_activities_createdDateUTC",
                table: "activities",
                newName: "IX_activities_CreatedAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_activities",
                table: "activities",
                column: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_friendships_distinct_users",
                table: "friendships",
                sql: "\"RequesterId\" <> \"AddresseeId\"");

            migrationBuilder.AddForeignKey(
                name: "FK_activities_dogs_DogId",
                table: "activities",
                column: "DogId",
                principalTable: "dogs",
                principalColumn: "dogID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_challenge_participants_challenges_ChallengeId",
                table: "challenge_participants",
                column: "ChallengeId",
                principalTable: "challenges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_challenge_participants_users_UserId",
                table: "challenge_participants",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_connected_accounts_users_UserId",
                table: "connected_accounts",
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
    }
}
