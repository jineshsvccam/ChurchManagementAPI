using ChurchContracts;
using ChurchData;
using ChurchServices;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TransacionHeadsTest
{
    public class TransactionHeadServiceTests
    {
        private readonly Mock<ITransactionHeadRepository> _mockRepository;
        private readonly Mock<ILogger<TransactionHeadService>> _mockLogger;
        private readonly TransactionHeadService _service;

        public TransactionHeadServiceTests()
        {
            _mockRepository = new Mock<ITransactionHeadRepository>();
            _mockLogger = new Mock<ILogger<TransactionHeadService>>();
            _service = new TransactionHeadService(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetTransactionHeadsAsync_ReturnsTransactionHeads()
        {
            // Arrange
            var transactionHeads = new List<TransactionHead>
            {
                new TransactionHead { HeadId = 1, HeadName = "Test" }
            };
            _mockRepository.Setup(repo => repo.GetTransactionHeadsAsync(null, null)).ReturnsAsync(transactionHeads);

            // Act
            var result = await _service.GetTransactionHeadsAsync(null, null);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTransactionHead()
        {
            // Arrange
            var transactionHead = new TransactionHead { HeadId = 1, HeadName = "Test" };
            _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(transactionHead);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.Equal(1, result?.HeadId);
        }

        [Fact]
        public async Task AddOrUpdateAsync_AddsOrUpdatesTransactionHeads()
        {
            // Arrange
            var transactionHeads = new List<TransactionHead>
            {
                new TransactionHead { HeadId = 1, HeadName = "Test", Action = "INSERT" },
                new TransactionHead { HeadId = 2, HeadName = "Test2", Action = "UPDATE" }
            };
            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<TransactionHead>())).ReturnsAsync((TransactionHead th) => th);
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<TransactionHead>())).ReturnsAsync((TransactionHead th) => th);

            // Act
            var result = await _service.AddOrUpdateAsync(transactionHeads);

            // Assert
           // Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddAsync_AddsTransactionHead()
        {
            // Arrange
            var transactionHead = new TransactionHead { HeadId = 1, HeadName = "Test" };
            _mockRepository.Setup(repo => repo.AddAsync(transactionHead)).ReturnsAsync(transactionHead);

            // Act
            var result = await _service.AddAsync(transactionHead);

            // Assert
            Assert.Equal(1, result.HeadId);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTransactionHead()
        {
            // Arrange
            var transactionHead = new TransactionHead { HeadId = 1, HeadName = "Test" };
            _mockRepository.Setup(repo => repo.UpdateAsync(transactionHead)).ReturnsAsync(transactionHead);

            // Act
            var result = await _service.UpdateAsync(transactionHead);

            // Assert
           // Assert.Equal(1, result.HeadId);
        }

        [Fact]
        public async Task DeleteAsync_DeletesTransactionHead()
        {
            // Arrange
            var transactionHeadId = 1;
            _mockRepository.Setup(repo => repo.DeleteAsync(transactionHeadId)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(transactionHeadId);

            // Assert
            _mockRepository.Verify(repo => repo.DeleteAsync(transactionHeadId), Times.Once);
        }
    }
}
