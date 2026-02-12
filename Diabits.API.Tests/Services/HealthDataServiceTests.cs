//using Diabits.API.Data;
//using Diabits.API.DTOs;
//using Diabits.API.DTOs.HealthDataPoints;
//using Diabits.API.Helpers.Mapping;
//using Diabits.API.Models;
//using Diabits.API.Models.HealthDataPoints;
//using Diabits.API.Models.HealthDataPoints.HealthConnect;
//using Diabits.API.Models.HealthDataPoints.ManualInput;
//using Diabits.API.Services;
//using Microsoft.EntityFrameworkCore;
//using Moq;

//namespace Diabits.API.Tests.Services;

//public sealed class HealthDataServiceTests : IDisposable
//{
//    private readonly DiabitsDbContext _db;
//    private readonly MapperFactory _mapperFactory;
//    private readonly HealthDataService _service;

//    private readonly DateTime _date = new(2026, 12, 31);
//    private readonly string _userId = "id";

//    public HealthDataServiceTests()
//    {
//        _db = CreateInMemoryDb();
//        _mockMapperFactory = new Mock<MapperFactory>(null!, null!, null!);
//        _service = new HealthDataService(_db, _mockMapperFactory.Object);

//        SeedTestUser();
//    }

//    public void Dispose()
//    {
//        _db.Dispose();
//    }

//    [Fact]
//    public async Task GetManualInputForDay_ReturnsCorrectData_WhenDataExists()
//    {
//        // Arrange
//        var medication = new Medication
//        {
//            UserId = _userId,
//            StartTime = _date,
//            EndTime = _date,
//            Name = "Panodil",
//            Quantity = 2,
//            StrengthValue = 500,
//            StrengthUnit = StrengthUnit.Mg
//        };
//        var menstruation = new Menstruation
//        {
//            UserId = _userId,
//            StartTime = _date,
//            EndTime = _date,
//            Flow = FlowEnum.MEDIUM
//        };

//        await _db.Set<Medication>().AddAsync(medication);
//        await _db.Set<Menstruation>().AddAsync(menstruation);
//        await _db.SaveChangesAsync();

//        // Act
//        var result = await _service.GetManualInputForDayAsync(_userId, _date);

//        // Assert
//        Assert.NotNull(result.Menstruation);
//        Assert.Single(result.Medications);
//        Assert.Equal(FlowEnum.MEDIUM, result.Menstruation.Flow);
//        Assert.Equal("Panodil", result.Medications[0]!.Name);
//    }

//    [Fact]
//    public async Task GetManualInputForDay_ReturnsEmpty_WhenNoDataForDay()
//    {
//        // Act
//        var result = await _service.GetManualInputForDayAsync(_userId, _date);

//        // Assert
//        Assert.Null(result.Menstruation);
//        Assert.Empty(result.Medications);
//    }

//    [Fact]
//    public async Task AddHealthConnectData_SuccessfullyAddsNewPoints()
//    {
//        // Arrange
//        var workoutDto = new WorkoutDto
//        {
//            HealthDataType = HealthDataType.WORKOUT,
//            DateFrom = DateTime.UtcNow.AddHours(-1),
//            DateTo = DateTime.UtcNow,
//            HealthValue = new WorkoutHealthValueDto 
//            {                
//                ActivityType = "Running",
//                CaloriesBurned = 300
//            }
//        };

//        var workout = new Workout
//        {
//            StartTime = workoutDto.DateFrom,
//            EndTime = workoutDto.DateTo,
//            ActivityType = "Running",
//            CaloriesBurned = 300,
//        };

//        _mockMapperFactory.Setup(m => m.FromDto(It.IsAny<WorkoutDto>())).Returns(workout);

//        var request = new HealthConnectRequest(
//            Workouts: [workoutDto],
//            Numerics: []
//        );

//        // Act
//        await _service.AddDataPointsAsync(request, _userId);

//        // Assert
//        _mockMapperFactory.Verify(m => m.FromDto(It.IsAny<WorkoutDto>()), Times.Once);

//        var saved = await _db.Set<Workout>().SingleAsync();
//        Assert.Equal(_userId, saved.UserId);
//        Assert.Equal("Running", saved.ActivityType);
//    }

//    [Fact]
//    public async Task AddHealthConnectData_DeduplicatesExistingPoints()
//    {
//        // Arrange
//        var startTime = DateTime.UtcNow.AddHours(-1);
//        var endTime = DateTime.UtcNow;

//        // Add existing workout
//        var existingWorkout = new Workout
//        {
//            UserId = UserId,
//            StartTime = startTime,
//            EndTime = endTime,
//            WorkoutType = "Running"
//        };
//        await _db.Set<Workout>().AddAsync(existingWorkout);
//        await _db.SaveChangesAsync();

//        // Try to add duplicate
//        var workoutDto = new WorkoutDto
//        {
//            HealthDataType = HealthDataType.WORKOUT,
//            StartTime = startTime,
//            EndTime = endTime,
//            WorkoutType = "Running"
//        };

//        var workout = new Workout
//        {
//            StartTime = workoutDto.StartTime,
//            EndTime = workoutDto.EndTime,
//            WorkoutType = workoutDto.WorkoutType
//        };

//        _mockMapperFactory.Setup(m => m.FromDto(It.IsAny<WorkoutDto>())).Returns(workout);

//        var request = new HealthConnectRequest(
//            Workouts: [workoutDto],
//            Numerics: []
//        );

//        // Act
//        await _service.AddDataPointsAsync(request, UserId);

//        // Assert
//        var savedWorkouts = await _db.Set<Workout>().ToListAsync();
//        Assert.Single(savedWorkouts); // Should still be only 1
//    }

//    #endregion

//    #region AddDataPointsAsync - Manual Input

//    [Fact]
//    public async Task AddManualInputData_SuccessfullyAddsNewEntries()
//    {
//        // Arrange
//        var medDto = new ManualInputDto
//        {
//            HealthDataType = HealthDataType.MEDICATION,
//            StartTime = DateTime.UtcNow,
//            EndTime = DateTime.UtcNow,
//            Medication = new MedicationDto("Ibuprofen", 1, 200, "MILLIGRAM")
//        };

//        var medication = new Medication
//        {
//            StartTime = medDto.StartTime,
//            EndTime = medDto.EndTime,
//            Name = medDto.Medication.Name,
//            Quantity = medDto.Medication.Quantity,
//            StrengthValue = medDto.Medication.StrengthValue,
//            StrengthUnit = StrengthUnit.MILLIGRAM
//        };

//        _mockMapperFactory.Setup(m => m.FromDto(It.IsAny<ManualInputDto>())).Returns(medication);

//        var request = new ManualInputRequest([medDto]);

//        // Act
//        await _service.AddDataPointsAsync(request, UserId);

//        // Assert
//        var savedMeds = await _db.Set<Medication>().ToListAsync();
//        Assert.Single(savedMeds);
//        Assert.Equal("Ibuprofen", savedMeds[0].Name);
//    }

//    #endregion

//    #region BatchUpdateManualInputAsync

//    [Fact]
//    public async Task BatchUpdate_UpdatesMedications_WhenValidIds()
//    {
//        // Arrange
//        var medication = new Medication
//        {
//            UserId = UserId,
//            StartTime = DateTime.UtcNow,
//            EndTime = DateTime.UtcNow,
//            Name = "OldName",
//            Quantity = 1,
//            StrengthValue = 100,
//            StrengthUnit = StrengthUnit.MILLIGRAM
//        };
//        await _db.Set<Medication>().AddAsync(medication);
//        await _db.SaveChangesAsync();

//        var updateDto = new ManualInputDto
//        {
//            Id = medication.Id,
//            HealthDataType = HealthDataType.MEDICATION,
//            StartTime = medication.StartTime,
//            EndTime = medication.EndTime,
//            Medication = new MedicationDto("NewName", 2, 200, "MILLIGRAM")
//        };

//        _mockMapperFactory.Setup(m => m.UpdateManualInput(It.IsAny<ManualInputDto>(), It.IsAny<Medication>()))
//            .Callback<ManualInputDto, Medication>((dto, med) =>
//            {
//                med.Name = dto.Medication!.Name;
//                med.Quantity = dto.Medication.Quantity;
//            });

//        // Act
//        var result = await _service.BatchUpdateManualInputAsync(UserId, [updateDto]);

//        // Assert
//        Assert.Equal(1, result);
//        var updated = await _db.Set<Medication>().FindAsync(medication.Id);
//        Assert.Equal("NewName", updated!.Name);
//        Assert.Equal(2, updated.Quantity);
//    }

//    [Fact]
//    public async Task BatchUpdate_UpdatesMenstruations_WhenValidIds()
//    {
//        // Arrange
//        var menstruation = new Menstruation
//        {
//            UserId = UserId,
//            StartTime = DateTime.UtcNow,
//            EndTime = DateTime.UtcNow,
//            Flow = FlowEnum.LIGHT
//        };
//        await _db.Set<Menstruation>().AddAsync(menstruation);
//        await _db.SaveChangesAsync();

//        var updateDto = new ManualInputDto
//        {
//            Id = menstruation.Id,
//            HealthDataType = HealthDataType.MENSTRUATION,
//            StartTime = menstruation.StartTime,
//            EndTime = menstruation.EndTime,
//            Flow = "HEAVY"
//        };

//        _mockMapperFactory.Setup(m => m.UpdateManualInput(It.IsAny<ManualInputDto>(), It.IsAny<Menstruation>()))
//            .Callback<ManualInputDto, Menstruation>((dto, mens) =>
//            {
//                mens.Flow = Enum.Parse<FlowEnum>(dto.Flow!);
//            });

//        // Act
//        var result = await _service.BatchUpdateManualInputAsync(UserId, [updateDto]);

//        // Assert
//        Assert.Equal(1, result);
//        var updated = await _db.Set<Menstruation>().FindAsync(menstruation.Id);
//        Assert.Equal(FlowEnum.HEAVY, updated!.Flow);
//    }

//    [Fact]
//    public async Task BatchUpdate_ReturnsZero_WhenNoValidIds()
//    {
//        // Arrange
//        var updateDto = new ManualInputDto
//        {
//            Id = null, // No ID
//            HealthDataType = HealthDataType.MEDICATION,
//            StartTime = DateTime.UtcNow,
//            EndTime = DateTime.UtcNow
//        };

//        // Act
//        var result = await _service.BatchUpdateManualInputAsync(UserId, [updateDto]);

//        // Assert
//        Assert.Equal(0, result);
//    }

//    #endregion

//    #region BatchDeleteManualInputAsync

//    [Fact]
//    public async Task BatchDelete_DeletesMultipleEntries_WhenValidIds()
//    {
//        // Arrange
//        var medication = new Medication
//        {
//            UserId = UserId,
//            StartTime = DateTime.UtcNow,
//            EndTime = DateTime.UtcNow,
//            Name = "Med1",
//            Quantity = 1,
//            StrengthValue = 100,
//            StrengthUnit = StrengthUnit.MILLIGRAM
//        };
//        var menstruation = new Menstruation
//        {
//            UserId = UserId,
//            StartTime = DateTime.UtcNow,
//            EndTime = DateTime.UtcNow,
//            Flow = FlowEnum.MEDIUM
//        };

//        await _db.Set<Medication>().AddAsync(medication);
//        await _db.Set<Menstruation>().AddAsync(menstruation);
//        await _db.SaveChangesAsync();

//        // Act
//        var result = await _service.BatchDeleteManualInputAsync(UserId, [medication.Id, menstruation.Id]);

//        // Assert
//        Assert.Equal(2, result);
//        Assert.Empty(await _db.Set<Medication>().ToListAsync());
//        Assert.Empty(await _db.Set<Menstruation>().ToListAsync());
//    }

//    [Fact]
//    public async Task BatchDelete_ReturnsZero_WhenNoMatchingIds()
//    {
//        // Act
//        var result = await _service.BatchDeleteManualInputAsync(UserId, [9999, 8888]);

//        // Assert
//        Assert.Equal(0, result);
//    }

//    #endregion

//    #region Helpers

//    private static DiabitsDbContext CreateInMemoryDb()
//    {
//        var options = new DbContextOptionsBuilder<DiabitsDbContext>()
//            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
//            .Options;

//        return new DiabitsDbContext(options);
//    }

//    private void SeedTestUser()
//    {
//        _db.Users.Add(new DiabitsUser { Id = UserId, UserName = "testuser" });
//        _db.SaveChanges();
//    }

//    #endregion
//}