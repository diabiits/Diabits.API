using Diabits.API.Data.Mapping;
using Diabits.API.DTOs.HealthDataPoints;
using Diabits.API.Helpers.Mapping;
using Diabits.API.Models.HealthDataPoints;
using Diabits.API.Models.HealthDataPoints.HealthConnect;
using Diabits.API.Models.HealthDataPoints.ManualInput;
using System;
using System.Collections.Generic;
using System.Text;

namespace Diabits.API.Tests.Helpers;

public class MapperFactoryTests
{
    private readonly MapperFactory _mapperFactory = new(
        new NumericMapper(),
        new WorkoutMapper(),
        new ManualInputMapper()
    );

    private readonly DateTime _date = new(2026, 12, 31);

    [Fact]
    public void FromDto_GlucoseLevelDto_MapsToGlucoseLevel()
    {
        var dto = new NumericDto
        {
            HealthDataType = HealthDataType.BLOOD_GLUCOSE,
            DateFrom = _date,
            HealthValue = new NumericHealthValueDto { NumericValue = 5.5 }
        };

        var entity = _mapperFactory.FromDto(dto);

        var glucoseLevel = Assert.IsType<GlucoseLevel>(entity);
        Assert.Equal(HealthDataType.BLOOD_GLUCOSE, glucoseLevel.Type);
        Assert.Equal(5.5, glucoseLevel.mmolL);
    }

    [Fact]
    public void FromDto_HeartRateDto_MapsToHeartRate()
    {
        var dto = new NumericDto
        {
            HealthDataType = HealthDataType.HEART_RATE,
            DateFrom = _date,
            HealthValue = new NumericHealthValueDto { NumericValue = 100 }
        };

        var entity = _mapperFactory.FromDto(dto);

        var heartRate = Assert.IsType<HeartRate>(entity);
        Assert.Equal(HealthDataType.HEART_RATE, heartRate.Type);
        Assert.Equal(100, heartRate.BPM);
    }

    [Fact]
    public void FromDto_StepDto_MapsToStep()
    {
        var dto = new NumericDto
        {
            HealthDataType = HealthDataType.STEPS,
            DateFrom = _date,
            DateTo = _date.AddHours(1),
            HealthValue = new NumericHealthValueDto { NumericValue = 500 }
        };

        var entity = _mapperFactory.FromDto(dto);

        var steps = Assert.IsType<Step>(entity);
        Assert.Equal(HealthDataType.STEPS, steps.Type);
        Assert.Equal(500, steps.Count);
    }

    [Fact]
    public void FromDto_SleepSessionDto_MapsToSleepSession()
    {
        var dto = new NumericDto
        {
            HealthDataType = HealthDataType.SLEEP_SESSION,
            DateFrom = _date,
            DateTo = _date.AddHours(8),
            HealthValue = new NumericHealthValueDto { NumericValue = 480 }
        };

        var entity = _mapperFactory.FromDto(dto);

        var sleepSession = Assert.IsType<SleepSession>(entity);
        Assert.Equal(HealthDataType.SLEEP_SESSION, sleepSession.Type);
        Assert.Equal(480, sleepSession.DurationMinutes);
    }

    [Fact]
    public void FromDto_WorkoutDto_MapsToWorkout()
    {
        var dto = new WorkoutDto
        {
            HealthDataType = HealthDataType.WORKOUT,
            DateFrom = _date,
            DateTo = _date.AddHours(1),
            HealthValue = new WorkoutHealthValueDto { ActivityType = "Running", CaloriesBurned = 300 }
        };

        var entity = _mapperFactory.FromDto(dto);

        var workout = Assert.IsType<Workout>(entity);
        Assert.Equal(HealthDataType.WORKOUT, workout.Type);
        Assert.Equal("Running", workout.ActivityType);
        Assert.Equal(300, workout.CaloriesBurned);
    }

    [Fact]
    public void FromDto_ManualDto_MapsToMedication()
    {
        var dto = new ManualInputDto
        {
            HealthDataType = HealthDataType.MEDICATION,
            DateFrom = _date,
            Medication = new MedicationValueDto
            {
                Name = "Panodil",
                Quantity = 2,
                StrengthValue = 500,
                StrengthUnit = "Mg"
            }
        };

        var entity = _mapperFactory.FromDto(dto);

        var med = Assert.IsType<Medication>(entity);
        Assert.Equal(HealthDataType.MEDICATION, med.Type);
        Assert.Equal("Panodil", med.Name);
        Assert.Equal(2, med.Quantity);
        Assert.Equal(500, med.StrengthValue);
        Assert.Equal(StrengthUnit.Mg, med.StrengthUnit);
    }

    [Fact]
    public void FromDto_MenstruationDto_MapsToMenstruation()
    {
        var dto = new ManualInputDto
        {
            HealthDataType = HealthDataType.MENSTRUATION,
            DateFrom = _date,
            Flow = FlowEnum.HEAVY
        };

        var entity = _mapperFactory.FromDto(dto);

        var mens = Assert.IsType<Menstruation>(entity);
        Assert.Equal(HealthDataType.MENSTRUATION, mens.Type);
        Assert.Equal(FlowEnum.HEAVY, mens.Flow);
    }

    [Fact]
    public void UpdateManualInput_MedicationDto_UpdatesMedication()
    {
        var existingMed = new Medication
        {
            Id = 1,
            Type = HealthDataType.MEDICATION,
            StartTime = _date,
            Name = "Panodil",
            Quantity = 2,
            StrengthValue = 500,
            StrengthUnit = StrengthUnit.Mg
        };

        var updateDto = new ManualInputDto
        {
            Id = 1,
            HealthDataType = HealthDataType.MEDICATION,
            DateFrom = _date,
            Medication = new MedicationValueDto
            {
                Name = "Ipren",
                Quantity = 1,
                StrengthValue = 200,
                StrengthUnit = "Mg"
            }
        };

        _mapperFactory.UpdateManualInput(updateDto, existingMed);

        Assert.Equal("Ipren", existingMed.Name);
        Assert.Equal(1, existingMed.Quantity);
        Assert.Equal(200, existingMed.StrengthValue);
        Assert.Equal(StrengthUnit.Mg, existingMed.StrengthUnit);
    }

    [Fact]
    public void UpdateManualInput_MenstruationDto_UpdatesMenstruation()
    {
        var existingMens = new Menstruation
        {
            Id = 1,
            Type = HealthDataType.MENSTRUATION,
            StartTime = _date,
            Flow = FlowEnum.LIGHT
        };

        var updateDto = new ManualInputDto
        {
            Id = 1,
            HealthDataType = HealthDataType.MENSTRUATION,
            DateFrom = _date,
            Flow = FlowEnum.HEAVY
        };
        _mapperFactory.UpdateManualInput(updateDto, existingMens);
        Assert.Equal(FlowEnum.HEAVY, existingMens.Flow);
    }
}