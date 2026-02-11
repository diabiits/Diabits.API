using Diabits.API.Data;
using Diabits.API.DTOs;
using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Interfaces;
using Diabits.API.Models.HealthDataPoints.ManualInput;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Diabits.API.Services;

public class HealthDataService : IHealthDataService
{
    private readonly DiabitsDbContext _dbContext;
    private readonly ILogger<HealthDataService> _logger;

    public HealthDataService(DiabitsDbContext dbContext, ILogger<HealthDataService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Returns manual input entries for a single day grouped into menstruation (single entry) and medication (list).
    /// </summary>
    public async Task<ManualInputResponse> GetManualInputForDayAsync(string userId, DateTime date)
    {
        //TODO Test that this still works as intended
        var startOfDay = date.AtMidnight();
        var endOfDay = startOfDay.AddDays(1).AddMicroseconds(-1); // inclusive end of day


       var meds = await _dbContext
            .Set<Medication>()
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.StartTime >= startOfDay && m.StartTime <= endOfDay)
            .ToListAsync();

        var mens = await _dbContext
            .Set<Menstruation>()
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.StartTime >= startOfDay && m.StartTime <= endOfDay)
            .FirstOrDefaultAsync();

        return new ManualInputResponse(
            Menstruation: mens,
            Medications: meds
            );

        //var items = await _dbContext.HealthDataPoints
        //      .AsNoTracking()
        //      .Where(h => h.UserId == userId &&
        //                  (h.Type == HealthDataType.MEDICATION || h.Type == HealthDataType.MENSTRUATION) &&
        //                  h.StartTime >= startOfDay &&
        //                  h.StartTime <= endOfDay)
        //      .ToListAsync();

        //// There should be at most one menstruation entry per day in this model — return the first if present.
        //var menstruation = items
        //    .Where(x => x.Type == HealthDataType.MENSTRUATION).FirstOrDefault();

        //// Collect medication entries (can be multiple) and cast to concrete base type for DTO mapping later.
        //var medication = items
        //    .Where(x => x.Type == HealthDataType.MEDICATION)
        //    .Cast<HealthDataPoint?>()
        //    .ToList();

        //return new ManualInputGroupedResponse(menstruation, medication);
    }

    /// <summary>
    /// Delete a manual input item belonging to the user. Returns true when deletion succeeded.
    /// </summary>
    public async Task<bool> DeleteManualInputAsync(string userId, int id)
    {
        //TODO Send type as well so we can query just the relevant table (medication vs menstruation) instead of the whole HealthDataPoints set
        var toDelete = await _dbContext.HealthDataPoints
            .Where(h => h.UserId == userId && h.Id == id)
            .FirstOrDefaultAsync();

        if (toDelete == null) return false;

        _dbContext.HealthDataPoints.Remove(toDelete);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    // Update a manual input entry for the user. Supports domain-specific updates for Medication and Menstruation.
    // Returns false when the entity is not found/doesn't belong to the user.
    public async Task<bool> UpdateManualInputAsync(string userId, ManualInputDto inputDto)
    {
        var existingEntity = await _dbContext.HealthDataPoints
            .Where(h => h.UserId == userId && h.Id == inputDto.Id)
            .FirstOrDefaultAsync();

        if (existingEntity == null) return false;

        // If the stored entity is a Medication and DTO contains medication data, update fields.
        if (existingEntity is Medication med && inputDto.Medication is not null)
        {
            med.Name = inputDto.Medication.Name;
            med.Quantity = inputDto.Medication.Quantity;
            med.StrengthValue = inputDto.Medication.StrengthValue;
            med.StrengthUnit = Enum.Parse<StrengthUnit>(inputDto.Medication.StrengthUnit, true);
        }

        // If the stored entity is a Menstruation and the DTO provides Flow, parse and set it.
        if (existingEntity is Menstruation men && inputDto.Flow is not null)
        {
            men.Flow = Enum.Parse<FlowEnum>(inputDto.Flow);
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    ///  Convert incoming DTOs (workouts + numerics) into mapped entity instances and persist them.
    ///  The mobile client already performs some deduplication; server still performs a safety dedupe.
    /// </summary>
    public async Task AddDataPointsAsync(HealthConnectRequest request, string userId)
    {
        // Ensure user exists and get a reference for updating last sync.
        var user = await _dbContext.Users.SingleAsync(u => u.Id == userId);

        var dataPoints = new List<HealthDataPoint>();

        // Map workout DTOs to domain entities using project-specific MapToEntity extensions/mappers.
        foreach (var w in request.Workouts)
            dataPoints.Add(w.MapToEntity());

        // Map numeric DTOs; MapToEntity may return null for unsupported or invalid items.
        foreach (var n in request.Numerics)
        {
            var dataPoint = n.MapToEntity();
            if (dataPoint != null)
                dataPoints.Add(dataPoint);
        }

        // Central add + dedupe routine
        await AddDataPointsAsync(dataPoints, user.Id);

        // Update user's last successful sync time with the client-provided time.
        user.LastSyncSuccess = request.ClientSyncTime;
        await _dbContext.SaveChangesAsync();
    }


    /// <summary>
    ///  Convert incoming manual input DTOs (menstruation + medications) into mapped entity instances and persist.
    /// </summary>
    public async Task MapDataPointsAsync(ManualInputRequest request, string userId)
    {
        var user = await _dbContext.Users.SingleAsync(u => u.Id == userId);

        var dataPoints = new List<HealthDataPoint>();

        // Map all items - MapToEntity determines the type based on populated properties
        foreach (var item in request.Items)
        {
            var dataPoint = item.MapToEntity();
            if (dataPoint != null)
                dataPoints.Add(dataPoint);
        }

        // Reuse central add + dedupe routine
        await AddDataPointsAsync(dataPoints, user.Id);
    }

    /// <summary>
    /// Mapped datapoints to DB (with deduplication against existing data)
    /// </summary>
    private async Task AddDataPointsAsync(List<HealthDataPoint> points, string userId)
    {
        if (points.Count == 0)
            return;

        // Determine sync window from incoming points to limit the existing-data query.
        var start = points.Min(p => p.StartTime);
        var end = points.Max(p => p.EndTime);

        // Load existing datapoints from the computed window for this user.
        // Note: existing query restricts by StartTime only (keep consistent with dedupe keys).
        var existingPoints = await _dbContext.HealthDataPoints
            .Where(dp => dp.UserId == userId)
            .Where(dp => dp.StartTime >= start)
            .ToListAsync();

        // Build lightweight keys for existing points used by IsDuplicate.
        // For Medication we include Name and Amount to handle medication-specific equality.
        var existingKeys = existingPoints.Select(dp =>
        {
            if (dp is Medication med)
            {
                return new ExistingKey
                {
                    Type = med.Type,
                    StartTime = med.StartTime,
                    MedicationName = med.Name,
                    Quantity = med.Quantity
                };
            }

            // Non-medication datapoints use only Type + StartTime as the equality basis.
            return new ExistingKey
            {
                Type = dp.Type,
                StartTime = dp.StartTime
            };

        }).ToList();

        // Filter incoming points against existingKeys to avoid inserting duplicates.
        var toAdd = points
            .Where(p => !IsDuplicate(p, existingKeys))
            .ToList();

        if (toAdd.Count == 0)
            return;

        // Assign ownership before persistence.
        foreach (var dp in toAdd)
            dp.UserId = userId;

        // Insert and persist only the new datapoints.
        await _dbContext.HealthDataPoints.AddRangeAsync(toAdd);
        await _dbContext.SaveChangesAsync();
    }

    // Deduplication helper: returns true when an incoming point already exists according to project rules.
    // Medication comparison checks Type, StartTime, Name and Amount.
    // Other types compare Type + StartTime.
    // TODO: Consider moving dedupe logic to a helper or more sophisticated matching (tolerances for time, merging).
    private static bool IsDuplicate(HealthDataPoint incoming, List<ExistingKey> existing)
    {
        if (incoming is Medication med)
        {
            return existing.Any(e =>
                e.Type == HealthDataType.MEDICATION &&
                e.StartTime == med.StartTime &&
                e.MedicationName == med.Name &&
                e.Quantity == med.Quantity);
        }

        // Default rule for other data types: match by Type and StartTime only.
        return existing.Any(e =>
            e.Type == incoming.Type &&
            e.StartTime == incoming.StartTime);
    }

    // Lightweight key used for comparison/deduplication of existing DB points.
    // Medication requires additional fields to avoid false duplicates.
    public class ExistingKey
    {
        public HealthDataType Type { get; set; }
        public DateTime StartTime { get; set; }

        // Only used for Medication comparisons
        public string? MedicationName { get; set; }
        public decimal? Quantity { get; set; }
    }

    // Update multiple manual input entries in a batch for the user.
    // Fetches all entities in one query, updates them in memory, then persists changes in a single transaction.
    // Returns the count of successfully updated entries.
    public async Task<int> BatchUpdateManualInputAsync(string userId, IEnumerable<ManualInputDto> inputDtos)
    {
        var dtoList = inputDtos.ToList();
        if (dtoList.Count == 0) return 0;

        // Extract all IDs from the DTOs (filter out null IDs)
        var ids = dtoList
            .Where(dto => dto.Id.HasValue)
            .Select(dto => dto.Id!.Value)
            .ToList();

        if (ids.Count == 0) return 0;

        // Fetch all matching entities in a single query for efficiency
        var existingEntities = await _dbContext.HealthDataPoints
            .Where(h => h.UserId == userId && ids.Contains(h.Id))
            .ToListAsync();

        var updateCount = 0;

        // Update entities in memory
        foreach (var inputDto in dtoList)
        {
            if (!inputDto.Id.HasValue) continue;

            var existingEntity = existingEntities.FirstOrDefault(e => e.Id == inputDto.Id.Value);
            if (existingEntity == null) continue;

            // If the stored entity is a Medication and DTO contains medication data, update fields.
            if (existingEntity is Medication med && inputDto.Medication is not null)
            {
                med.Name = inputDto.Medication.Name;
                med.Quantity = inputDto.Medication.Quantity;
                med.StrengthValue = inputDto.Medication.StrengthValue;
                med.StrengthUnit = Enum.Parse<StrengthUnit>(inputDto.Medication.StrengthUnit, true);
                updateCount++;
            }
            // If the stored entity is a Menstruation and the DTO provides Flow, parse and set it.
            else if (existingEntity is Menstruation men && inputDto.Flow is not null)
            {
                men.Flow = Enum.Parse<FlowEnum>(inputDto.Flow);
                updateCount++;
            }
        }

        // Persist all changes in a single transaction
        if (updateCount > 0)
            await _dbContext.SaveChangesAsync();

        return updateCount;
    }

    // Delete multiple manual input entries in a batch belonging to the user.
    // Fetches all entities in one query, removes them, then persists changes in a single transaction.
    // Returns the count of successfully deleted entries.
    public async Task<int> BatchDeleteManualInputAsync(string userId, IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return 0;

        // Fetch all entities to delete in a single query
        var toDelete = await _dbContext.HealthDataPoints
            .Where(h => h.UserId == userId && idList.Contains(h.Id))
            .ToListAsync();

        if (toDelete.Count == 0) return 0;

        // Use RemoveRange for efficiency
        _dbContext.HealthDataPoints.RemoveRange(toDelete);
        await _dbContext.SaveChangesAsync();

        return toDelete.Count;
    }
}