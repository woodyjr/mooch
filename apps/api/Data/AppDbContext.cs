using Mooch.Api.Entities.Activities;
using Mooch.Api.Entities.Challenges;
using Mooch.Api.Entities.Dogs;
using Mooch.Api.Entities.Friendships;
using Mooch.Api.Entities.Integrations;
using Mooch.Api.Entities.Users;
using Mooch.Api.Entities.Walkers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mooch.Api.Data;

public sealed class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityDog> ActivityDogs => Set<ActivityDog>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<ChallengeParticipant> ChallengeParticipants => Set<ChallengeParticipant>();
    public DbSet<ConnectedAccount> ConnectedAccounts => Set<ConnectedAccount>();
    public DbSet<Dog> Dogs => Set<Dog>();
    public DbSet<DogOwnerInvite> DogOwnerInvites => Set<DogOwnerInvite>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<ImportedActivity> ImportedActivities => Set<ImportedActivity>();
    public DbSet<Walker> Walkers => Set<Walker>();
    public DbSet<WalkerDog> WalkerDogs => Set<WalkerDog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.Property(x => x.DisplayName).HasMaxLength(100);
            entity.Property(x => x.AvatarImage).HasMaxLength(2000);
        });

        builder.Entity<Walker>(entity =>
        {
            entity.ToTable("walkers");
            entity.HasKey(x => x.WalkerID);
            entity.Property(x => x.WalkerID).HasColumnName("walkerID");
            entity.Property(x => x.UserID).HasColumnName("userID");
            entity.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.AvatarImage).HasMaxLength(2000);
            entity.Property(x => x.CreatedDateUtc).HasColumnName("createdDateUTC");
            entity.HasOne(x => x.User)
                .WithOne(x => x.Walker)
                .HasForeignKey<Walker>(x => x.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Dog>(entity =>
        {
            entity.ToTable("dogs");
            entity.HasKey(x => x.DogID);
            entity.Property(x => x.DogID).HasColumnName("dogID");
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Breed).HasMaxLength(120);
            entity.Property(x => x.Bio).HasMaxLength(500);
            entity.Property(x => x.AvatarImage).HasMaxLength(2000);
            entity.Property(x => x.CreatedDateUtc).HasColumnName("createdDateUTC");
        });

        builder.Entity<DogOwnerInvite>(entity =>
        {
            entity.ToTable("dog_owner_invites");
            entity.HasKey(x => x.DogOwnerInviteID);
            entity.Property(x => x.DogOwnerInviteID).HasColumnName("dogOwnerInviteID");
            entity.Property(x => x.DogID).HasColumnName("dogID");
            entity.Property(x => x.InvitedByWalkerID).HasColumnName("invitedByWalkerID");
            entity.Property(x => x.InviteeEmail).HasColumnName("inviteeEmail").HasMaxLength(320).IsRequired();
            entity.Property(x => x.CreatedDateUtc).HasColumnName("createdDateUTC");
            entity.Property(x => x.RespondedDateUtc).HasColumnName("respondedDateUTC");
            entity.HasIndex(x => x.DogID);
            entity.HasIndex(x => new { x.DogID, x.InviteeEmail, x.Status })
                .HasDatabaseName("IX_dog_owner_invites_pending_email");
            entity.HasOne(x => x.Dog)
                .WithMany(x => x.OwnerInvites)
                .HasForeignKey(x => x.DogID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.InvitedByWalker)
                .WithMany(x => x.SentDogOwnerInvites)
                .HasForeignKey(x => x.InvitedByWalkerID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WalkerDog>(entity =>
        {
            entity.ToTable("walker_dogs");
            entity.HasKey(x => x.WalkerDogID);
            entity.Property(x => x.WalkerDogID).HasColumnName("walkerDogID");
            entity.Property(x => x.WalkerID).HasColumnName("walkerID");
            entity.Property(x => x.DogID).HasColumnName("dogID");
            entity.Property(x => x.CreatedDateUtc).HasColumnName("createdDateUTC");
            entity.HasIndex(x => x.DogID);
            entity.HasIndex(x => new { x.WalkerID, x.DogID }).IsUnique();
            entity.HasIndex(x => x.DogID)
                .HasDatabaseName("IX_walker_dogs_primary_owner")
                .HasFilter("\"IsPrimaryOwner\" = true")
                .IsUnique();
            entity.HasOne(x => x.Walker)
                .WithMany(x => x.WalkerDogs)
                .HasForeignKey(x => x.WalkerID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Dog)
                .WithMany(x => x.WalkerDogs)
                .HasForeignKey(x => x.DogID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<IdentityRole<Guid>>().ToTable("roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");

        builder.Entity<Activity>(entity =>
        {
            entity.ToTable("activities");
            entity.HasKey(x => x.ActivityID);
            entity.Property(x => x.ActivityID).HasColumnName("activityID");
            entity.Property(x => x.WalkerID).HasColumnName("walkerID");
            entity.Property(x => x.ConnectedAccountID).HasColumnName("connectedAccountID");
            entity.Property(x => x.ExternalActivityID).HasColumnName("externalActivityID").HasMaxLength(200);
            entity.Property(x => x.Title).HasMaxLength(140).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.DistanceMiles).HasPrecision(6, 2);
            entity.Property(x => x.CreatedDateUtc).HasColumnName("createdDateUTC");
            entity.HasIndex(x => new { x.WalkerID, x.StartedAtUtc });
            entity.HasIndex(x => x.CreatedDateUtc);
            entity.HasIndex(x => new { x.ConnectedAccountID, x.ExternalActivityID })
                .HasFilter("\"connectedAccountID\" IS NOT NULL AND \"externalActivityID\" IS NOT NULL")
                .IsUnique();
            entity.HasOne(x => x.Walker)
                .WithMany(x => x.Activities)
                .HasForeignKey(x => x.WalkerID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ConnectedAccount)
                .WithMany(x => x.Activities)
                .HasForeignKey(x => x.ConnectedAccountID)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ActivityDog>(entity =>
        {
            entity.ToTable("activity_dogs");
            entity.HasKey(x => x.ActivityDogID);
            entity.Property(x => x.ActivityDogID).HasColumnName("activityDogID");
            entity.Property(x => x.ActivityID).HasColumnName("activityID");
            entity.Property(x => x.DogID).HasColumnName("dogID");
            entity.Property(x => x.CreatedDateUtc).HasColumnName("createdDateUTC");
            entity.HasIndex(x => x.ActivityID);
            entity.HasIndex(x => x.DogID);
            entity.HasIndex(x => new { x.ActivityID, x.DogID }).IsUnique();
            entity.HasIndex(x => new { x.DogID, x.CreatedDateUtc });
            entity.HasOne(x => x.Activity)
                .WithMany(x => x.ActivityDogs)
                .HasForeignKey(x => x.ActivityID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Dog)
                .WithMany(x => x.ActivityDogs)
                .HasForeignKey(x => x.DogID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ImportedActivity>(entity =>
        {
            entity.ToTable("imported_activities");
            entity.HasKey(x => x.ImportedActivityID);
            entity.Property(x => x.ImportedActivityID).HasColumnName("importedActivityID");
            entity.Property(x => x.WalkerID).HasColumnName("walkerID");
            entity.Property(x => x.ConnectedAccountID).HasColumnName("connectedAccountID");
            entity.Property(x => x.ExternalActivityID).HasColumnName("externalActivityID").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(140).IsRequired();
            entity.Property(x => x.ActivityType).HasColumnName("activityType").HasMaxLength(40).IsRequired();
            entity.Property(x => x.DistanceMiles).HasPrecision(6, 2);
            entity.Property(x => x.CreatedDateUtc).HasColumnName("createdDateUTC");
            entity.Property(x => x.RequiresDogAssignment).HasColumnName("requiresDogAssignment");
            entity.HasIndex(x => new { x.WalkerID, x.StartedAtUtc });
            entity.HasIndex(x => new { x.ConnectedAccountID, x.ExternalActivityID }).IsUnique();
            entity.HasOne(x => x.Walker)
                .WithMany()
                .HasForeignKey(x => x.WalkerID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ConnectedAccount)
                .WithMany()
                .HasForeignKey(x => x.ConnectedAccountID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Friendship>(entity =>
        {
            entity.ToTable("friendships", table => table.HasCheckConstraint("CK_friendships_distinct_users", "\"requesterUserID\" <> \"addresseeUserID\""));
            entity.HasKey(x => x.FriendshipID);
            entity.Property(x => x.FriendshipID).HasColumnName("friendshipID");
            entity.Property(x => x.RequesterUserID).HasColumnName("requesterUserID");
            entity.Property(x => x.AddresseeUserID).HasColumnName("addresseeUserID");
            entity.Property(x => x.CreatedDateUtc).HasColumnName("createdDateUTC");
            entity.HasIndex(x => new { x.RequesterUserID, x.AddresseeUserID }).IsUnique();
            entity.HasIndex(x => new { x.AddresseeUserID, x.Status });
            entity.HasOne(x => x.Requester)
                .WithMany(x => x.SentFriendRequests)
                .HasForeignKey(x => x.RequesterUserID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Addressee)
                .WithMany(x => x.ReceivedFriendRequests)
                .HasForeignKey(x => x.AddresseeUserID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConnectedAccount>(entity =>
        {
            entity.ToTable("connected_accounts");
            entity.HasKey(x => x.ConnectedAccountID);
            entity.Property(x => x.ConnectedAccountID).HasColumnName("connectedAccountID");
            entity.Property(x => x.UserID).HasColumnName("userID");
            entity.Property(x => x.ExternalUserID).HasColumnName("externalUserID").HasMaxLength(200).IsRequired();
            entity.Property(x => x.AccessToken).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.RefreshToken).HasMaxLength(4000);
            entity.Property(x => x.CreatedDateUtc).HasColumnName("createdDateUTC");
            entity.HasIndex(x => new { x.UserID, x.Provider }).IsUnique();
            entity.HasIndex(x => new { x.Provider, x.ExternalUserID }).IsUnique();
            entity.HasOne(x => x.User)
                .WithMany(x => x.ConnectedAccounts)
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Challenge>(entity =>
        {
            entity.ToTable("challenges", table => table.HasCheckConstraint("CK_challenges_date_range", "\"EndsOn\" >= \"StartsOn\""));
            entity.HasKey(x => x.ChallengeID);
            entity.Property(x => x.ChallengeID).HasColumnName("challengeID");
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(600);
        });

        builder.Entity<ChallengeParticipant>(entity =>
        {
            entity.ToTable("challenge_participants");
            entity.HasKey(x => x.ChallengeParticipantID);
            entity.Property(x => x.ChallengeParticipantID).HasColumnName("challengeParticipantID");
            entity.Property(x => x.ChallengeID).HasColumnName("challengeID");
            entity.Property(x => x.UserID).HasColumnName("userID");
            entity.HasIndex(x => new { x.ChallengeID, x.UserID }).IsUnique();
            entity.HasIndex(x => x.UserID);
            entity.HasOne(x => x.Challenge)
                .WithMany(x => x.Participants)
                .HasForeignKey(x => x.ChallengeID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User)
                .WithMany(x => x.ChallengeParticipants)
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
