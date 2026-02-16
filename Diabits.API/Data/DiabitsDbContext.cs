using Diabits.API.Models;
using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.HealthConnect;
using Diabits.API.Models.HealthDataPoints.ManualInput;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Diabits.API.Data;

public class DiabitsDbContext : IdentityDbContext<DiabitsUser>
{
    public DiabitsDbContext(DbContextOptions<DiabitsDbContext> options) : base(options) { }

    public DbSet<HealthDataPoint> HealthDataPoints { get; set; }
    public DbSet<Invite> Invites { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Use Table-per-Concrete-type mapping for the HealthDataPoint inheritance hierarchy
        // This stores each concrete derived type in its own table while allowing an abstract parent class for shared properties
        builder.Entity<HealthDataPoint>().UseTpcMappingStrategy();

        builder.Entity<GlucoseLevel>().ToTable("GlucoseLevels");
        builder.Entity<HeartRate>().ToTable("HeartRates");
        builder.Entity<SleepSession>().ToTable("SleepSessions");
        builder.Entity<Step>().ToTable("Steps");
        builder.Entity<Workout>().ToTable("Workouts");
        builder.Entity<Menstruation>().ToTable("Menstruation");
        builder.Entity<Medication>().ToTable("Medications");

        // Index to optimize queries that look up menstruation entries per user and day window
        //TODO Add for others?
        builder.Entity<Menstruation>().HasIndex(m => new { m.UserId, m.StartTime } );

        builder.Entity<GlucoseLevel>().Property(g => g.mmolL).HasPrecision(3, 1);

        builder.Entity<Invite>()
            .Property(i => i.UsedAt)
            .HasField("_usedAt");

        builder.Entity<Invite>()
            .HasIndex(i => i.Email)
            .IsUnique();

        builder.Entity<Invite>()
            .HasOne(i => i.UsedBy)
            .WithOne(u => u.Invite)
            .HasForeignKey<DiabitsUser>(u => u.InviteId)
            .IsRequired(false);

        builder.Entity<RefreshToken>()
            .HasIndex(r => r.TokenHash)
            .IsUnique();
    }
}