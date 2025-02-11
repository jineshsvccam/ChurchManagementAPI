using Xunit;
using Microsoft.EntityFrameworkCore;
using ChurchData;
using ChurchRepositories;
using ChurchServices;
using ChurchContracts;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DistrictsTest
{
    public class DistrictServiceIntegrationTests : IAsyncLifetime
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DistrictRepository _districtRepository;
        private readonly DistrictService _districtService;
        private readonly Mock<ILogger<DistrictService>> _mockLogger; // Mock the logger

        public DistrictServiceIntegrationTests()
        {
            _mockLogger = new Mock<ILogger<DistrictService>>(); // Initialize the mock logger
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ChurchDb")
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _districtRepository = new DistrictRepository(_dbContext);
            _districtService = new DistrictService(_districtRepository, _mockLogger.Object);
        }

        public async Task InitializeAsync()
        {
            await ClearDatabase();
        }

        public async Task DisposeAsync()
        {
            await ClearDatabase();
        }

        private async Task ClearDatabase()
        {
            _dbContext.Districts.RemoveRange(_dbContext.Districts);
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfDistricts()
        {
            // Arrange
            _dbContext.Districts.AddRange(new List<District>
            {
                new District { DistrictId = 1, DistrictName = "District A", Description = "Description A" },
                new District { DistrictId = 2, DistrictName = "District B", Description = "Description B" }
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _districtService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDistrict_WhenDistrictExists()
        {
            // Arrange
            var districtId = 1;
            var district = new District { DistrictId = districtId, DistrictName = "District A", Description = "Description A" };
            _dbContext.Districts.Add(district);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _districtService.GetByIdAsync(districtId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(districtId, result.DistrictId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDistrictDoesNotExist()
        {
            // Arrange
            var districtId = 99;

            // Act
            var result = await _districtService.GetByIdAsync(districtId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddDistrict()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "District A", Description = "Description A" };

            // Act
            await _districtService.AddAsync(district);
            var result = await _dbContext.Districts.FindAsync(district.DistrictId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(district.DistrictId, result.DistrictId);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateDistrict()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "District A", Description = "Description A" };
            _dbContext.Districts.Add(district);
            await _dbContext.SaveChangesAsync();

            // Act
            district.DistrictName = "Updated District A";
            district.Description = "Updated Description";
            await _districtService.UpdateAsync(district);
            var result = await _dbContext.Districts.FindAsync(district.DistrictId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated District A", result.DistrictName);
            Assert.Equal("Updated Description", result.Description);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveDistrict()
        {
            // Arrange
            await ClearDatabase(); // Ensure the database is cleared before adding a new district
            var district = new District { DistrictId = 1, DistrictName = "District A", Description = "Description A" };
            _dbContext.Districts.Add(district);
            await _dbContext.SaveChangesAsync();

            // Act
            await _districtService.DeleteAsync(district.DistrictId);
            var result = await _dbContext.Districts.FindAsync(district.DistrictId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenDistrictDoesNotExist()
        {
            // Arrange
            var districtId = 99;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _districtService.DeleteAsync(districtId));
            Assert.Equal("District not found", exception.Message);
        }
    }
}
