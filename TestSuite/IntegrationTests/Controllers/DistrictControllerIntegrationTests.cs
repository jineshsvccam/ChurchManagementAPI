using Xunit;
using Microsoft.EntityFrameworkCore;
using ChurchData;
using ChurchRepositories;
using ChurchServices;
using ChurchManagementAPI.Controllers;
using ChurchContracts;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DistrictsTest
{
    public class DistrictControllerIntegrationTests : IAsyncLifetime
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DistrictRepository _districtRepository;
        private readonly DistrictService _districtService;
        private readonly DistrictController _districtController;
        private readonly Mock<ILogger<DistrictService>> _logger;

        public DistrictControllerIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ChurchDb")
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _logger = new Mock<ILogger<DistrictService>>();
            _districtRepository = new DistrictRepository(_dbContext);
            _districtService = new DistrictService(_districtRepository, _logger.Object);
            _districtController = new DistrictController(_districtService);
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
        public async Task GetAll_ShouldReturnOkResult_WithListOfDistricts()
        {
            // Arrange
            _dbContext.Districts.AddRange(new List<District>
            {
                new District { DistrictId = 1, DistrictName = "District A", Description = "Description A" },
                new District { DistrictId = 2, DistrictName = "District B", Description = "Description B" }
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _districtController.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<District>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnOkResult_WithDistrict()
        {
            // Arrange
            var districtId = 1;
            var district = new District { DistrictId = districtId, DistrictName = "District A", Description = "Description A" };
            _dbContext.Districts.Add(district);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _districtController.GetById(districtId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<District>(okResult.Value);
            Assert.Equal(districtId, returnValue.DistrictId);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction_WhenDistrictIsValid()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "District A", Description = "Description A" };

            // Act
            var result = await _districtController.Create(district);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<District>(createdAtActionResult.Value);
            Assert.Equal(district.DistrictId, returnValue.DistrictId);
        }

        [Fact]
        public async Task Update_ShouldReturnOkResult_WithUpdatedDistrict()
        {
            // Arrange
            var districtId = 1;
            var district = new District { DistrictId = districtId, DistrictName = "District A", Description = "Description A" };
            _dbContext.Districts.Add(district);
            await _dbContext.SaveChangesAsync();

            district.DistrictName = "Updated District A";
            district.Description = "Updated Description";

            // Act
            var result = await _districtController.Update(districtId, district);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<District>(okResult.Value);
            Assert.Equal("Updated District A", returnValue.DistrictName);
            Assert.Equal("Updated Description", returnValue.Description);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenDistrictIsDeleted()
        {
            // Arrange
           // await ClearDatabase(); // Ensure the database is cleared before adding a new district
            var district = new District { DistrictId = 1, DistrictName = "District A", Description = "Description A" };
            _dbContext.Districts.Add(district);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _districtController.Delete(district.DistrictId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
