using Xunit;
using Microsoft.EntityFrameworkCore;
using ChurchData;
using ChurchRepositories.Admin;
using ChurchContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistrictsTest
{
    public class DistrictRepositoryTests : IAsyncLifetime
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DistrictRepository _districtRepository;

        public DistrictRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique database name for each test run
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _districtRepository = new DistrictRepository(_dbContext);
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
            var result = await _districtRepository.GetAllAsync();

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
            var result = await _districtRepository.GetByIdAsync(districtId);

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
            var result = await _districtRepository.GetByIdAsync(districtId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddDistrict()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "District A", Description = "Description A" };

            // Act
            await _districtRepository.AddAsync(district);
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
            await _districtRepository.UpdateAsync(district);
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
            var district = new District { DistrictId = 1, DistrictName = "District A", Description = "Description A" };
            _dbContext.Districts.Add(district);
            await _dbContext.SaveChangesAsync();

            // Act
            await _districtRepository.DeleteAsync(district.DistrictId);
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
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _districtRepository.DeleteAsync(districtId));
            Assert.Equal("District not found", exception.Message);
        }

    }
}
