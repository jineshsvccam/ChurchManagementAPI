using ChurchContracts;
using ChurchData;
using ChurchManagementAPI.Controllers;
using ChurchServices;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DistrictsTest
{
    public class DistrictControllerTests
    {
        private readonly Mock<IDistrictService> _mockDistrictService;
        private readonly DistrictController _controller;

        public DistrictControllerTests()
        {
            _mockDistrictService = new Mock<IDistrictService>();
            _controller = new DistrictController(_mockDistrictService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfDistricts()
        {
            // Arrange
            var districts = new List<District> { new District { DistrictId = 1, DistrictName = "District1" } };
            _mockDistrictService.Setup(service => service.GetAllAsync()).ReturnsAsync(districts);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<District>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WithDistrict()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "District1" };
            _mockDistrictService.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(district);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<District>(okResult.Value);
            Assert.Equal(1, returnValue.DistrictId);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenDistrictNotFound()
        {
            // Arrange
            _mockDistrictService.Setup(service => service.GetByIdAsync(1)).ReturnsAsync((District)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "District1" };
            _mockDistrictService.Setup(service => service.AddAsync(district)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(district);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdAtActionResult.ActionName);
            Assert.Equal(1, createdAtActionResult.RouteValues["id"]);
        }

        [Fact]
        public async Task Update_ReturnsOkResult_WithUpdatedDistrict()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "UpdatedDistrict" };
            _mockDistrictService.Setup(service => service.UpdateAsync(district)).Returns(Task.CompletedTask);
            _mockDistrictService.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(district);

            // Act
            var result = await _controller.Update(1, district);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<District>(okResult.Value);
            Assert.Equal("UpdatedDistrict", returnValue.DistrictName);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "District1" };

            // Act
            var result = await _controller.Update(2, district);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContentResult()
        {
            // Arrange
            _mockDistrictService.Setup(service => service.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
