using ChurchContracts;
using ChurchData;
using ChurchManagementAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TransacionHeadsTest
{
    public class TransactionHeadControllerTests
    {
        private readonly Mock<ITransactionHeadService> _mockService;
        private readonly Mock<ILogger<TransactionHeadController>> _mockLogger;
        private readonly TransactionHeadController _controller;

        public TransactionHeadControllerTests()
        {
            _mockService = new Mock<ITransactionHeadService>();
            _mockLogger = new Mock<ILogger<TransactionHeadController>>();
            _controller = new TransactionHeadController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetTransactionHeads_ReturnsOkResult_WithTransactionHeads()
        {
            // Arrange
            var transactionHeads = new List<TransactionHead> { new TransactionHead { HeadId = 1, HeadName = "Test" } };
            _mockService.Setup(service => service.GetTransactionHeadsAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                        .ReturnsAsync(transactionHeads);

            // Act
            var result = await _controller.GetTransactionHeads(null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<TransactionHead>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WithTransactionHead()
        {
            // Arrange
            var transactionHead = new TransactionHead { HeadId = 1, HeadName = "Test" };
            _mockService.Setup(service => service.GetByIdAsync(It.IsAny<int>()))
                        .ReturnsAsync(transactionHead);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<TransactionHead>(okResult.Value);
            Assert.Equal(1, returnValue.HeadId);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenTransactionHeadNotFound()
        {
            // Arrange
            _mockService.Setup(service => service.GetByIdAsync(It.IsAny<int>()))
                        .ReturnsAsync((TransactionHead)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateOrUpdate_ReturnsCreatedAtActionResult_WhenTransactionHeadsCreatedOrUpdated()
        {
            // Arrange
            var transactionHeads = new List<TransactionHead> { new TransactionHead { HeadId = 1, HeadName = "Test" } };
            _mockService.Setup(service => service.AddOrUpdateAsync(It.IsAny<IEnumerable<TransactionHead>>()))
                        .ReturnsAsync(transactionHeads);

            // Act
            var result = await _controller.CreateOrUpdate(transactionHeads);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<List<TransactionHead>>(createdAtActionResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenTransactionHeadUpdated()
        {
            // Arrange
            var transactionHead = new TransactionHead { HeadId = 1, HeadName = "Test" };
            _mockService.Setup(service => service.UpdateAsync(It.IsAny<TransactionHead>()))
                        .ReturnsAsync(transactionHead);

            // Act
            var result = await _controller.Update(1, transactionHead);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var transactionHead = new TransactionHead { HeadId = 2, HeadName = "Test" };

            // Act
            var result = await _controller.Update(1, transactionHead);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID mismatch", badRequestResult.Value);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenTransactionHeadDeleted()
        {
            // Arrange
            _mockService.Setup(service => service.DeleteAsync(It.IsAny<int>()))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
