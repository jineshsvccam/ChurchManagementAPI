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

namespace DiocesesTest
{
    public class DioceseControllerIntegrationTests : IAsyncLifetime
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DioceseRepository _dioceseRepository;
        private readonly DioceseService _dioceseService;
        private readonly DioceseController _dioceseController;
       

        public DioceseControllerIntegrationTests()
        {
            var mockLoggerRepository = new Mock<ILogger<DioceseRepository>>();
            var mockLoggerService = new Mock<ILogger<DioceseService>>();
            var mockLoggerController = new Mock<ILogger<DioceseController>>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ChurchDb")
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _dioceseRepository = new DioceseRepository(_dbContext, mockLoggerRepository.Object);
            _dioceseService = new DioceseService(_dioceseRepository, mockLoggerService.Object);
            _dioceseController = new DioceseController(_dioceseService, mockLoggerController.Object);
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
            _dbContext.Dioceses.RemoveRange(_dbContext.Dioceses);
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkResult_WithListOfDioceses()
        {
            // Arrange
            _dbContext.Dioceses.AddRange(new List<Diocese>
        {
            new Diocese { DioceseId = 1, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" },
            new Diocese { DioceseId = 2, DioceseName = "Diocese B", Address = "Address B", ContactInfo = "Contact B", Territory = "Territory B" }
        });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _dioceseController.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Diocese>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnOkResult_WithDiocese()
        {
            // Arrange
            var dioceseId = 1;
            var diocese = new Diocese { DioceseId = dioceseId, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" };
            _dbContext.Dioceses.Add(diocese);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _dioceseController.GetById(dioceseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Diocese>(okResult.Value);
            Assert.Equal(dioceseId, returnValue.DioceseId);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction_WhenDioceseIsValid()
        {
            // Arrange
            var diocese = new Diocese { DioceseId = 1, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" };

            // Act
            var result = await _dioceseController.Create(diocese);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Diocese>(createdAtActionResult.Value);
            Assert.Equal(diocese.DioceseId, returnValue.DioceseId);
        }

        [Fact]
        public async Task Update_ShouldReturnOkResult_WithUpdatedDiocese()
        {
            // Arrange
            var dioceseId = 1;
            var diocese = new Diocese { DioceseId = dioceseId, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" };
            _dbContext.Dioceses.Add(diocese);
            await _dbContext.SaveChangesAsync();

            diocese.DioceseName = "Updated Diocese A";
            diocese.Address = "Updated Address";
            diocese.ContactInfo = "Updated Contact";
            diocese.Territory = "Updated Territory";

            // Act
            var result = await _dioceseController.Update(dioceseId, diocese);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Diocese>(okResult.Value);
            Assert.Equal("Updated Diocese A", returnValue.DioceseName);
            Assert.Equal("Updated Address", returnValue.Address);
            Assert.Equal("Updated Contact", returnValue.ContactInfo);
            Assert.Equal("Updated Territory", returnValue.Territory);
        }


        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenDioceseIsDeleted()
        {
            // Arrange
            var diocese = new Diocese { DioceseId = 1, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" };
            _dbContext.Dioceses.Add(diocese);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _dioceseController.Delete(diocese.DioceseId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
