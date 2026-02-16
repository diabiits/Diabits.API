using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.HealthConnect;
using Riok.Mapperly.Abstractions;

namespace Diabits.API.Data.Mapping;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class WorkoutMapper
{
    [MapProperty(nameof(NumericDto.DateFrom), nameof(HealthDataPoint.StartTime))]
    [MapProperty(nameof(NumericDto.DateTo), nameof(HealthDataPoint.EndTime))]
    [MapProperty(nameof(NumericDto.HealthDataType), nameof(HealthDataPoint.Type))]
    [MapProperty(nameof(WorkoutDto.HealthValue) + "." + nameof(WorkoutHealthValueDto.ActivityType), nameof(Workout.ActivityType))]
    [MapProperty(nameof(WorkoutDto.HealthValue) + "." + nameof(WorkoutHealthValueDto.CaloriesBurned), nameof(Workout.CaloriesBurned))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.UserId))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.User))]
    public partial Workout ToWorkout(WorkoutDto dto);

    [MapProperty(nameof(HealthDataPoint.StartTime), nameof(WorkoutDto.DateFrom))]
    [MapProperty(nameof(HealthDataPoint.EndTime), nameof(WorkoutDto.DateTo))]
    [MapProperty(nameof(HealthDataPoint.Type), nameof(WorkoutDto.HealthDataType))]
    [MapProperty(nameof(Workout.ActivityType), nameof(WorkoutDto.HealthValue) + "." + nameof(WorkoutHealthValueDto.ActivityType))]
    [MapProperty(nameof(Workout.CaloriesBurned), nameof(WorkoutDto.HealthValue) + "." + nameof(WorkoutHealthValueDto.CaloriesBurned))]
    public partial WorkoutDto ToDto(Workout entity);

    private int Map(double? value) => value.HasValue ? (int)Math.Round(value.Value) : 0;

    private double? MapToNullableDouble(int value) => value > 0 ? value : null;
}
