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

        // Use Table-per-Concrete-type mapping for HealthDataPoint
        // This stores each concrete derived type in its own table
        // while allowing an abstract parent class for shared properties
        builder.Entity<HealthDataPoint>().UseTpcMappingStrategy();

        builder.Entity<GlucoseLevel>()
            .ToTable("GlucoseLevels")
            .HasIndex(g => new { g.UserId, g.StartTime });

        builder.Entity<HeartRate>()
            .ToTable("HeartRates")
            .HasIndex(h => new { h.UserId, h.StartTime});

        builder.Entity<SleepSession>()
            .ToTable("SleepSessions")
            .HasIndex(s => new { s.UserId, s.StartTime });

        builder.Entity<Step>()
            .ToTable("Steps")
            .HasIndex(s => new { s.UserId, s.StartTime });

        builder.Entity<Workout>()
            .ToTable("Workouts")
            .HasIndex(w => new { w.UserId, w.StartTime });

        builder.Entity<Menstruation>()
            .ToTable("Menstruation")
            .HasIndex(m => new { m.UserId, m.StartTime });

        builder.Entity<Medication>()
            .ToTable("Medications")
            .HasIndex(m => new { m.UserId, m.StartTime });

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