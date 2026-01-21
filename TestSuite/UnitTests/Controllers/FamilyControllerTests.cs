using ChurchContracts;
using ChurchManagementAPI.Controllers.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using ChurchData;

namespace FamilyTests
{
    public class FamilyControllerTests
    {
        private readonly Mock<IFamilyService> _mockFamilyService;
        private readonly Mock<ILogger<FamilyController>> _mockLogger;
        private readonly FamilyController _controller;

        public FamilyControllerTests()
        {
            _mockFamilyService = new Mock<IFamilyService>();
            _mockLogger = new Mock<ILogger<FamilyController>>();
            _controller = new FamilyController(_mockFamilyService.Object, _mockLogger.Object);
        }

        #region GetFamilies Tests

        [Fact]
        public async Task GetFamilies_ShouldReturnOk_WithFamilies()
        {
            // Arrange
            var families = new List<Family>
            {
                new Family { FamilyId = 1, FamilyName = "Smith" },
                new Family { FamilyId = 2, FamilyName = "Johnson" }
            };

            _mockFamilyService.Setup(s => s.GetFamiliesAsync(null, null, null))
                .ReturnsAsync(families);

            // Act
            var result = await _controller.GetFamilies(null, null, null);
            var okResult = result.Result as OkObjectResult;

            // Assert
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(families);
        }

        [Fact]
        public async Task GetFamilies_ShouldReturn500_WhenExceptionOccurs()
        {
            // Arrange
            _mockFamilyService.Setup(s => s.GetFamiliesAsync(null, null, null))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetFamilies(null, null, null) as ActionResult<IEnumerable<Family>>;

            // Assert
            result.Result.Should().NotBeNull();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("An error occurred while retrieving families.");
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFamilyExists()
        {
            // Arrange
            var family = new Family { FamilyId = 1, FamilyName = "Smith" };
            _mockFamilyService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(family);

            // Act
            var result = await _controller.GetById(1);
            var okResult = result.Result as OkObjectResult;

            // Assert
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(family);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenFamilyDoesNotExist()
        {
            // Arrange
            _mockFamilyService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((Family)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>(); // Fix: Check the `.Result` property
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("Family with Id 1 not found.");
        }

        [Fact]
        public async Task GetById_ShouldReturn500_WhenExceptionOccurs()
        {
            // Arrange
            _mockFamilyService.Setup(s => s.GetByIdAsync(1))
                // Act
                .ThrowsAsync(new Exception("Test exception"));

            var result = await _controller.GetById(1);
            var objectResult = result.Result as ObjectResult;

            // Assert
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("An error occurred while retrieving the family.");
        }

        #endregion

        #region CreateOrUpdate Tests

        [Fact]
        public async Task CreateOrUpdate_ShouldReturnCreatedAtAction_WhenFamiliesAreCreated()
        {
            // Arrange
            var families = new List<Family> { new Family { FamilyId = 1, FamilyName = "Smith" } };
            _mockFamilyService.Setup(s => s.AddOrUpdateAsync(families)).ReturnsAsync(families);

            // Act
            var result = await _controller.CreateOrUpdate(families) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateOrUpdate_ShouldReturnBadRequest_WhenArgumentExceptionOccurs()
        {
            // Arrange
            var families = new List<Family>();
            _mockFamilyService.Setup(s => s.AddOrUpdateAsync(families))
                .ThrowsAsync(new ArgumentException("Invalid data"));

            // Act
            var result = await _controller.CreateOrUpdate(families) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            result.Value.Should().Be("Invalid data");
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var family = new Family { FamilyId = 1, FamilyName = "Smith" };
            _mockFamilyService.Setup(s => s.UpdateAsync(family)).ReturnsAsync(family);

            // Act
            var result = await _controller.Update(1, family) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            result.Value.Should().BeEquivalentTo(family);
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var family = new Family { FamilyId = 2, FamilyName = "Johnson" };

            // Act
            var result = await _controller.Update(1, family) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            result.Value.Should().Be("Family ID in the request body does not match the URL parameter.");
        }

        [Fact]
        public async Task Update_ShouldReturn500_WhenExceptionOccurs()
        {
            // Arrange
            var family = new Family { FamilyId = 1, FamilyName = "Smith" };
            _mockFamilyService.Setup(s => s.UpdateAsync(family))
                .ThrowsAsync(new Exception("Update failed"));

            // Act
            var result = await _controller.Update(1, family) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);
            result.Value.Should().Be("An error occurred while updating the family.");
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenSuccessful()
        {
            // Act
            var result = await _controller.Delete(1) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task Delete_ShouldReturn500_WhenExceptionOccurs()
        {
            // Arrange
            _mockFamilyService.Setup(s => s.DeleteAsync(1))
                .ThrowsAsync(new Exception("Delete failed"));

            // Act
            var result = await _controller.Delete(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);
            result.Value.Should().Be("An error occurred while deleting the family.");
        }

        #endregion
    }
}
