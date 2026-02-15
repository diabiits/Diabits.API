using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.ManualInput;

namespace Diabits.API.Helpers;

public class DeduplicationStrategy(IEnumerable<HealthDataPoint> existingPoints)
{
    private readonly HashSet<DeduplicationKey> _existingKeys = existingPoints.Select(CreateKey).ToHashSet();

    private static DeduplicationKey CreateKey(HealthDataPoint dataPoint)
    {
        return dataPoint switch
        {
            Medication med => new DeduplicationKey(med.Type, med.StartTime, med.Name),
            _ => new DeduplicationKey(dataPoint.Type, dataPoint.StartTime)
        };
    }
    public bool IsDuplicate(HealthDataPoint incoming) => _existingKeys.Contains(CreateKey(incoming));


    private record DeduplicationKey(HealthDataType Type, DateTime StartTime, string? MedicationName = null);
}
