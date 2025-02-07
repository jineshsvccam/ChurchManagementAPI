using Xunit;
using Moq;
using ChurchContracts;
using ChurchData;
using ChurchManagementAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiocesesTest
{
    public class DioceseControllerTests
    {
        private readonly Mock<IDioceseService> _dioceseServiceMock;
        private readonly DioceseController _dioceseController;

        public DioceseControllerTests()
        {
            _dioceseServiceMock = new Mock<IDioceseService>();
            _dioceseController = new DioceseController(_dioceseServiceMock.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkResult_WithListOfDioceses()
        {
            // Arrange
            var dioceses = new List<Diocese>
        {
            new Diocese { DioceseId = 1, DioceseName = "Diocese A" },
            new Diocese { DioceseId = 2, DioceseName = "Diocese B" }
        };
            _dioceseServiceMock.Setup(service => service.GetAllAsync()).ReturnsAsync(dioceses);

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
            var diocese = new Diocese { DioceseId = dioceseId, DioceseName = "Diocese A" };
            _dioceseServiceMock.Setup(service => service.GetByIdAsync(dioceseId)).ReturnsAsync(diocese);

            // Act
            var result = await _dioceseController.GetById(dioceseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Diocese>(okResult.Value);
            Assert.Equal(dioceseId, returnValue.DioceseId);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenDioceseDoesNotExist()
        {
            // Arrange
            var dioceseId = 1;
            _dioceseServiceMock.Setup(service => service.GetByIdAsync(dioceseId)).ReturnsAsync((Diocese)null);

            // Act
            var result = await _dioceseController.GetById(dioceseId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction_WhenDioceseIsValid()
        {
            // Arrange
            var diocese = new Diocese { DioceseId = 1, DioceseName = "Diocese A" };

            // Act
            var result = await _dioceseController.Create(diocese);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Diocese>(createdAtActionResult.Value);
            Assert.Equal(diocese.DioceseId, returnValue.DioceseId);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _dioceseController.ModelState.AddModelError("DioceseName", "Required");

            // Act
            var result = await _dioceseController.Create(new Diocese());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }


        [Fact]
        public async Task Update_ShouldReturnOkResult_WithUpdatedDiocese()
        {
            // Arrange
            var dioceseId = 1;
            var diocese = new Diocese { DioceseId = dioceseId, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" };
            var updatedDiocese = new Diocese { DioceseId = dioceseId, DioceseName = "Updated Diocese A", Address = "Updated Address", ContactInfo = "Updated Contact", Territory = "Updated Territory" };

            _dioceseServiceMock.Setup(service => service.UpdateAsync(diocese)).Returns(Task.CompletedTask);
            _dioceseServiceMock.Setup(service => service.GetByIdAsync(dioceseId)).ReturnsAsync(updatedDiocese);

            // Act
            var result = await _dioceseController.Update(dioceseId, diocese);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Diocese>(okResult.Value);
            Assert.Equal(updatedDiocese.DioceseName, returnValue.DioceseName);
        }


        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenDioceseIdMismatch()
        {
            // Arrange
            var dioceseId = 1;
            var diocese = new Diocese { DioceseId = 2, DioceseName = "Diocese A", Address = "Address A", ContactInfo = "Contact A", Territory = "Territory A" };

            // Act
            var result = await _dioceseController.Update(dioceseId, diocese);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestResult>(result.Result);
        }


        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenDioceseIsDeleted()
        {
            // Arrange
            var dioceseId = 1;

            // Act
            var result = await _dioceseController.Delete(dioceseId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
