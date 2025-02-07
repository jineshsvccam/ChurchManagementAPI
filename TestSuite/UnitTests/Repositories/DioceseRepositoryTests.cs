using Xunit;
using Microsoft.EntityFrameworkCore;
using ChurchData;
using ChurchRepositories;
using ChurchContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DioceseRepositoryTests : IAsyncLifetime
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DioceseRepository _dioceseRepository;

    public DioceseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique database name for each test run
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _dioceseRepository = new DioceseRepository(_dbContext);
    }

    public Task InitializeAsync()
    {
        // Nothing to initialize
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        // Cleanup resources
        _dbContext.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnListOfDioceses()
    {
        // Arrange
        _dbContext.Dioceses.AddRange(new List<Diocese>
        {
            new Diocese { DioceseId = 1, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" },
            new Diocese { DioceseId = 2, DioceseName = "Diocese B", Address = "Address B", ContactInfo = "Contact B", Territory = "Territory B" }
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _dioceseRepository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDiocese_WhenDioceseExists()
    {
        // Arrange
        var dioceseId = 1;
        var diocese = new Diocese { DioceseId = dioceseId, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" };
        _dbContext.Dioceses.Add(diocese);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _dioceseRepository.GetByIdAsync(dioceseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dioceseId, result.DioceseId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDioceseDoesNotExist()
    {
        // Arrange
        var dioceseId = 99;

        // Act
        var result = await _dioceseRepository.GetByIdAsync(dioceseId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddDiocese()
    {
        // Arrange
        var diocese = new Diocese { DioceseId = 1, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" };

        // Act
        await _dbContext.Dioceses.AddAsync(diocese);
        await _dbContext.SaveChangesAsync();

        var result = await _dbContext.Dioceses.FindAsync(diocese.DioceseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(diocese.DioceseId, result.DioceseId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDiocese()
    {
        // Arrange
        var diocese = new Diocese { DioceseId = 1, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" };
        _dbContext.Dioceses.Add(diocese);
        await _dbContext.SaveChangesAsync();

        // Act
        diocese.DioceseName = "Updated Diocese A";
        await _dioceseRepository.UpdateAsync(diocese);
        var result = await _dbContext.Dioceses.FindAsync(diocese.DioceseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Diocese A", result.DioceseName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveDiocese()
    {
        // Arrange
        var diocese = new Diocese { DioceseId = 1, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" };
        _dbContext.Dioceses.Add(diocese);
        await _dbContext.SaveChangesAsync();

        // Act
        await _dioceseRepository.DeleteAsync(diocese.DioceseId);
        var result = await _dbContext.Dioceses.FindAsync(diocese.DioceseId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenDioceseDoesNotExist()
    {
        // Arrange
        var dioceseId = 99;

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _dioceseRepository.DeleteAsync(dioceseId));
    }
}
