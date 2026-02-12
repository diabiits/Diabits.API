using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.ManualInput;
using Riok.Mapperly.Abstractions;

namespace Diabits.API.Data.Mapping;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ManualInputMapper
{
    [MapProperty(nameof(ManualInputDto.DateFrom), nameof(HealthDataPoint.StartTime))]
    [MapProperty(nameof(ManualInputDto.DateTo), nameof(HealthDataPoint.EndTime))]
    [MapProperty(nameof(ManualInputDto.HealthDataType), nameof(HealthDataPoint.Type))]
    [MapProperty(nameof(ManualInputDto.Medication) + "." + nameof(MedicationValueDto.Name), nameof(Medication.Name))]
    [MapProperty(nameof(ManualInputDto.Medication) + "." + nameof(MedicationValueDto.Quantity), nameof(Medication.Quantity))]
    [MapProperty(nameof(ManualInputDto.Medication) + "." + nameof(MedicationValueDto.StrengthValue), nameof(Medication.StrengthValue))]
    [MapProperty(nameof(ManualInputDto.Medication) + "." + nameof(MedicationValueDto.StrengthUnit), nameof(Medication.StrengthUnit))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.UserId))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.User))]
    public partial Medication ToMedication(ManualInputDto dto);

    [MapProperty(nameof(ManualInputDto.Medication) + "." + nameof(MedicationValueDto.Name), nameof(Medication.Name))]
    [MapProperty(nameof(ManualInputDto.Medication) + "." + nameof(MedicationValueDto.Quantity), nameof(Medication.Quantity))]
    [MapProperty(nameof(ManualInputDto.Medication) + "." + nameof(MedicationValueDto.StrengthValue), nameof(Medication.StrengthValue))]
    [MapProperty(nameof(ManualInputDto.Medication) + "." + nameof(MedicationValueDto.StrengthUnit), nameof(Medication.StrengthUnit))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.Id))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.UserId))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.User))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.Type))]
    public partial void UpdateMedication(ManualInputDto source, Medication target);

    [MapProperty(nameof(ManualInputDto.DateFrom), nameof(HealthDataPoint.StartTime))]
    [MapProperty(nameof(ManualInputDto.DateTo), nameof(HealthDataPoint.EndTime))]
    [MapProperty(nameof(ManualInputDto.HealthDataType), nameof(HealthDataPoint.Type))]
    [MapProperty(nameof(ManualInputDto.Flow), nameof(Menstruation.Flow))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.UserId))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.User))]
    public partial Menstruation ToMenstruation(ManualInputDto dto);

    [MapProperty(nameof(ManualInputDto.Flow), nameof(Menstruation.Flow))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.Id))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.UserId))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.User))]
    [MapperIgnoreTarget(nameof(HealthDataPoint.Type))]
    public partial void UpdateMenstruation(ManualInputDto source, Menstruation target);

    private StrengthUnit Map(string unit) => Enum.Parse<StrengthUnit>(unit, true);

    private FlowEnum MapFlow(string flow) => Enum.Parse<FlowEnum>(flow, true);
}
