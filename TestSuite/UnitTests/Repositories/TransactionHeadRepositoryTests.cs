using ChurchData;
using ChurchData.DTOs;
using ChurchRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TransacionHeadsTest
{
    public class TransactionHeadRepositoryTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<IOptions<LoggingSettings>> _mockLoggingSettings;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<ILogger<TransactionHeadRepository>> _mockLogger;
        private readonly TransactionHeadRepository _repository;

        public TransactionHeadRepositoryTests()
        {
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _mockLoggingSettings = new Mock<IOptions<LoggingSettings>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockUserManager = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _mockLogger = new Mock<ILogger<TransactionHeadRepository>>();

            _mockLoggingSettings.Setup(x => x.Value).Returns(new LoggingSettings
            {
                EnableChangeLogging = true,
                TableLogging = new Dictionary<string, bool> { { "transaction_heads", true } }
            });

            _repository = new TransactionHeadRepository(
                _mockContext.Object,
                _mockLoggingSettings.Object,
                _mockUserManager.Object,
                _mockHttpContextAccessor.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetTransactionHeadsAsync_ReturnsTransactionHeads()
        {
            // Arrange
            var transactionHeads = new List<TransactionHead>
            {
                new TransactionHead { HeadId = 1, HeadName = "Test" }
            };
            var mockSet = new Mock<DbSet<TransactionHead>>();
            mockSet.As<IQueryable<TransactionHead>>().Setup(m => m.Provider).Returns(transactionHeads.AsQueryable().Provider);
            mockSet.As<IQueryable<TransactionHead>>().Setup(m => m.Expression).Returns(transactionHeads.AsQueryable().Expression);
            mockSet.As<IQueryable<TransactionHead>>().Setup(m => m.ElementType).Returns(transactionHeads.AsQueryable().ElementType);
            mockSet.As<IQueryable<TransactionHead>>().Setup(m => m.GetEnumerator()).Returns(transactionHeads.AsQueryable().GetEnumerator());

            _mockContext.Setup(c => c.TransactionHeads).Returns(mockSet.Object);

            // Act
            var result = await _repository.GetTransactionHeadsAsync(null, null);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTransactionHead()
        {
            // Arrange
            var transactionHead = new TransactionHead { HeadId = 1, HeadName = "Test" };
            _mockContext.Setup(c => c.TransactionHeads.FindAsync(1)).ReturnsAsync(transactionHead);

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.Equal(1, result?.HeadId);
        }

        [Fact]
        public async Task AddAsync_AddsTransactionHead()
        {
            // Arrange
            var transactionHead = new TransactionHead { HeadId = 1, HeadName = "Test" };
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("testuser");

            // Act
            var result = await _repository.AddAsync(transactionHead);

            // Assert
            _mockContext.Verify(c => c.TransactionHeads.AddAsync(transactionHead, default), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
            Assert.Equal(1, result.HeadId);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTransactionHead()
        {
            // Arrange
            var transactionHead = new TransactionHead { HeadId = 1, HeadName = "Test" };
            var mockSet = new Mock<DbSet<TransactionHead>>();
            mockSet.As<IQueryable<TransactionHead>>().Setup(m => m.Provider).Returns(new List<TransactionHead> { transactionHead }.AsQueryable().Provider);
            mockSet.As<IQueryable<TransactionHead>>().Setup(m => m.Expression).Returns(new List<TransactionHead> { transactionHead }.AsQueryable().Expression);
            mockSet.As<IQueryable<TransactionHead>>().Setup(m => m.ElementType).Returns(new List<TransactionHead> { transactionHead }.AsQueryable().ElementType);
            mockSet.As<IQueryable<TransactionHead>>().Setup(m => m.GetEnumerator()).Returns(new List<TransactionHead> { transactionHead }.AsQueryable().GetEnumerator());

            _mockContext.Setup(c => c.TransactionHeads).Returns(mockSet.Object);
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("testuser");

            // Act
            var result = await _repository.UpdateAsync(transactionHead);

            // Assert
            _mockContext.Verify(c => c.TransactionHeads.Update(transactionHead), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
            Assert.Equal(1, result.HeadId);
        }

        [Fact]
        public async Task DeleteAsync_DeletesTransactionHead()
        {
            // Arrange
            var transactionHead = new TransactionHead { HeadId = 1, HeadName = "Test" };
            _mockContext.Setup(c => c.TransactionHeads.FindAsync(1)).ReturnsAsync(transactionHead);
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("testuser");

            // Act
            await _repository.DeleteAsync(1);

            // Assert
            _mockContext.Verify(c => c.TransactionHeads.Remove(transactionHead), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }
    }
}
