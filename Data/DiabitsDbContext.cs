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
        builder.Entity<Menstruation>().HasIndex(m => new { m.UserId, m.StartTime } );

        // Map the Invite.UsedAt property to the private backing field "_usedAt"
        builder.Entity<Invite>()
            .Property(i => i.UsedAt)
            .HasField("_usedAt");
    }
}