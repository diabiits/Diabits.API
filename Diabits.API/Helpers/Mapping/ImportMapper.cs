using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.ManualInput;

using Riok.Mapperly.Abstractions;

namespace Diabits.API.Helpers.Mapping;

[Mapper]
public partial class ImportMapper
{
    [MapProperty(nameof(ImportDto.DateFrom), nameof(InsulinBolus.StartTime))]
    [MapProperty(nameof(ImportDto.DateTo), nameof(InsulinBolus.EndTime))]
    [MapProperty(nameof(ImportDto.HealthDataType), nameof(InsulinBolus.Type))]
    [MapProperty(nameof(ImportDto.Units), nameof(InsulinBolus.Units))]
    [MapProperty(nameof(ImportDto.CarbGrams), nameof(InsulinBolus.CarbGrams))]
    [MapProperty(nameof(ImportDto.GlucoseLevel), nameof(InsulinBolus.GlucoseLevel))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.UserId))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.User))]
    public partial InsulinBolus ToInsulinBolus(ImportDto dto);

    [MapProperty(nameof(InsulinBolus.StartTime), nameof(ImportDto.DateFrom))]
    [MapProperty(nameof(InsulinBolus.EndTime), nameof(ImportDto.DateTo))]
    [MapProperty(nameof(InsulinBolus.Type), nameof(ImportDto.HealthDataType))]
    public partial ImportDto ToDto(InsulinBolus entity);
}
