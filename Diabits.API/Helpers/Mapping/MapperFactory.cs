using Diabits.API.Data.Mapping;
using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.HealthConnect;
using Diabits.API.Models.HealthDataPoints.ManualInput;

namespace Diabits.API.Helpers.Mapping;

public sealed class MapperFactory
{
    private readonly NumericMapper _numericMapper;
    private readonly WorkoutMapper _workoutMapper;
    private readonly ManualInputMapper _manualInputMapper;


    public MapperFactory(NumericMapper numericMapper, WorkoutMapper workoutMapper, ManualInputMapper manualInputMapper)
    {
        _numericMapper = numericMapper;
        _workoutMapper = workoutMapper;
        _manualInputMapper = manualInputMapper;
    }

    public HealthDataPoint FromDto(NumericDto dto)
    {
        return dto.HealthDataType switch
        {
            HealthDataType.BLOOD_GLUCOSE => _numericMapper.ToGlucoseLevel(dto),
            HealthDataType.HEART_RATE => _numericMapper.ToHeartRate(dto),
            HealthDataType.STEPS => _numericMapper.ToStep(dto),
            HealthDataType.SLEEP_SESSION => _numericMapper.ToSleepSession(dto),
            _ => throw new InvalidOperationException($"Unsupported numeric type: {dto.HealthDataType}")
        };
    }

    public HealthDataPoint FromDto(WorkoutDto dto) => _workoutMapper.ToWorkout(dto);

    public HealthDataPoint FromDto(ManualInputDto dto)
    {
        return dto.HealthDataType switch
        {
            HealthDataType.MEDICATION => _manualInputMapper.ToMedication(dto),
            HealthDataType.MENSTRUATION => _manualInputMapper.ToMenstruation(dto),
            _ => throw new InvalidOperationException($"Unsupported numeric type: {dto.HealthDataType}")
        };
    }

    public void UpdateManualInput(ManualInputDto dto, Medication target)
        => _manualInputMapper.UpdateMedication(dto, target);

    public void UpdateManualInput(ManualInputDto dto, Menstruation target)
        => _manualInputMapper.UpdateMenstruation(dto, target);

    public HealthDataPointBaseDto ToDto(HealthDataPoint entity)
    {
        return entity switch
        {
            GlucoseLevel glucose => _numericMapper.ToDto(glucose),
            HeartRate heartRate => _numericMapper.ToDto(heartRate),
            Step step => _numericMapper.ToDto(step),
            SleepSession sleep => _numericMapper.ToDto(sleep),
            Workout workout => _workoutMapper.ToDto(workout),
            Medication medication => _manualInputMapper.ToDto(medication),
            Menstruation menstruation => _manualInputMapper.ToDto(menstruation),
            _ => throw new InvalidOperationException($"Unsupported entity type: {entity.GetType().Name}")
        };
    }
}
