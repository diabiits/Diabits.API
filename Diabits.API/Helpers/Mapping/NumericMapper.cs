using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.HealthConnect;
using Riok.Mapperly.Abstractions;

namespace Diabits.API.Data.Mapping;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class NumericMapper
{
    [MapProperty(nameof(NumericDto.DateFrom), nameof(HealthDataPoint.StartTime))]
    [MapProperty(nameof(NumericDto.DateTo), nameof(HealthDataPoint.EndTime))]
    [MapProperty(nameof(NumericDto.HealthDataType), nameof(HealthDataPoint.Type))]
    [MapProperty(nameof(NumericDto.HealthValue) + "." + nameof(NumericHealthValueDto.NumericValue), nameof(GlucoseLevel.mmolL))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.UserId))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.User))]
    public partial GlucoseLevel ToGlucoseLevel(NumericDto dto);

    [MapProperty(nameof(NumericDto.DateFrom), nameof(HealthDataPoint.StartTime))]
    [MapProperty(nameof(NumericDto.DateTo), nameof(HealthDataPoint.EndTime))]
    [MapProperty(nameof(NumericDto.HealthDataType), nameof(HealthDataPoint.Type))]
    [MapProperty(nameof(NumericDto.HealthValue) + "." + nameof(NumericHealthValueDto.NumericValue), nameof(HeartRate.BPM))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.UserId))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.User))]
    public partial HeartRate ToHeartRate(NumericDto dto);


    [MapProperty(nameof(NumericDto.DateFrom), nameof(HealthDataPoint.StartTime))]
    [MapProperty(nameof(NumericDto.DateTo), nameof(HealthDataPoint.EndTime))]
    [MapProperty(nameof(NumericDto.HealthDataType), nameof(HealthDataPoint.Type))]
    [MapProperty(nameof(NumericDto.HealthValue) + "." + nameof(NumericHealthValueDto.NumericValue), nameof(Step.Count))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.UserId))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.User))]
    public partial Step ToStep(NumericDto dto);

    [MapProperty(nameof(NumericDto.DateFrom), nameof(HealthDataPoint.StartTime))]
    [MapProperty(nameof(NumericDto.DateTo), nameof(HealthDataPoint.EndTime))]
    [MapProperty(nameof(NumericDto.HealthDataType), nameof(HealthDataPoint.Type))]
    [MapProperty(nameof(NumericDto.HealthValue) + "." + nameof(NumericHealthValueDto.NumericValue), nameof(SleepSession.DurationMinutes))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.UserId))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.User))]
    public partial SleepSession ToSleepSession(NumericDto dto);

    [MapProperty(nameof(HealthDataPoint.StartTime), nameof(NumericDto.DateFrom))]
    [MapProperty(nameof(HealthDataPoint.EndTime), nameof(NumericDto.DateTo))]
    [MapProperty(nameof(HealthDataPoint.Type), nameof(NumericDto.HealthDataType))]
    [MapProperty(nameof(GlucoseLevel.mmolL), nameof(NumericDto.HealthValue) + "." + nameof(NumericHealthValueDto.NumericValue))]
    public partial NumericDto ToDto(GlucoseLevel entity);

    [MapProperty(nameof(HealthDataPoint.StartTime), nameof(NumericDto.DateFrom))]
    [MapProperty(nameof(HealthDataPoint.EndTime), nameof(NumericDto.DateTo))]
    [MapProperty(nameof(HealthDataPoint.Type), nameof(NumericDto.HealthDataType))]
    [MapProperty(nameof(HeartRate.BPM), nameof(NumericDto.HealthValue) + "." + nameof(NumericHealthValueDto.NumericValue))]
    public partial NumericDto ToDto(HeartRate entity);

    [MapProperty(nameof(HealthDataPoint.StartTime), nameof(NumericDto.DateFrom))]
    [MapProperty(nameof(HealthDataPoint.EndTime), nameof(NumericDto.DateTo))]
    [MapProperty(nameof(HealthDataPoint.Type), nameof(NumericDto.HealthDataType))]
    [MapProperty(nameof(Step.Count), nameof(NumericDto.HealthValue) + "." + nameof(NumericHealthValueDto.NumericValue))]
    public partial NumericDto ToDto(Step entity);

    [MapProperty(nameof(HealthDataPoint.StartTime), nameof(NumericDto.DateFrom))]
    [MapProperty(nameof(HealthDataPoint.EndTime), nameof(NumericDto.DateTo))]
    [MapProperty(nameof(HealthDataPoint.Type), nameof(NumericDto.HealthDataType))]
    [MapProperty(nameof(SleepSession.DurationMinutes), nameof(NumericDto.HealthValue) + "." + nameof(NumericHealthValueDto.NumericValue))]
    public partial NumericDto ToDto(SleepSession entity);

    private int Map(double value) => (int)Math.Round(value);

    private double Map(decimal value) => (double)value;
}
