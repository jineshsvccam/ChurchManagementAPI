using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using ChurchServices;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FamilyTests
{
    public class FamilyServiceTests
    {
        private readonly Mock<IFamilyRepository> _mockFamilyRepository;
        private readonly Mock<ILogger<FamilyService>> _mockLogger;
        private readonly FamilyService _familyService;

        public FamilyServiceTests()
        {
            _mockFamilyRepository = new Mock<IFamilyRepository>();
            _mockLogger = new Mock<ILogger<FamilyService>>();
            _familyService = new FamilyService(_mockFamilyRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetFamiliesAsync_ShouldReturnFamilies_WhenTheyExist()
        {
            // Arrange
            var families = new List<Family>
        {
            new Family { FamilyId = 1, FamilyName = "John Family", ParishId = 1, UnitId = 1 },
            new Family { FamilyId = 2, FamilyName = "Doe Family", ParishId = 2, UnitId = 2 }
        };
            _mockFamilyRepository.Setup(repo => repo.GetFamiliesAsync(null, null, null))
                .ReturnsAsync(families);

            // Act
            var result = await _familyService.GetFamiliesAsync(null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnFamily_WhenExists()
        {
            // Arrange
            var family = new Family { FamilyId = 1, FamilyName = "John Family", ParishId = 1, UnitId = 1 };
            _mockFamilyRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(family);

            // Act
            var result = await _familyService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Family", result.FamilyName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            _mockFamilyRepository.Setup(repo => repo.GetByIdAsync(99))
                .ReturnsAsync((Family?)null);

            // Act
            var result = await _familyService.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddOrUpdateAsync_ShouldInsert_WhenActionIsInsert()
        {
            // Arrange
            var request = new Family { FamilyId = 1, FamilyName = "New Family", ParishId = 1, UnitId = 1, Action = "INSERT" };
            _mockFamilyRepository.Setup(repo => repo.AddAsync(request))
                .ReturnsAsync(request);

            // Act
            var result = await _familyService.AddOrUpdateAsync(new List<Family> { request });

            // Assert
            Assert.Single(result);
            Assert.Equal("New Family", result.First().FamilyName);
        }

        [Fact]
        public async Task AddOrUpdateAsync_ShouldUpdate_WhenActionIsUpdate()
        {
            // Arrange
            var request = new Family { FamilyId = 1, FamilyName = "Updated Family", ParishId = 1, UnitId = 1, Action = "UPDATE" };
            _mockFamilyRepository.Setup(repo => repo.UpdateAsync(request))
                .ReturnsAsync(request);

            // Act
            var result = await _familyService.AddOrUpdateAsync(new List<Family> { request });

            // Assert
            Assert.Single(result);
            Assert.Equal("Updated Family", result.First().FamilyName);
        }

        [Fact]
        public async Task AddOrUpdateAsync_ShouldThrowException_WhenActionIsInvalid()
        {
            // Arrange
            var request = new Family { FamilyId = 1, FamilyName = "Invalid Family", ParishId = 1, UnitId = 1, Action = "DELETE" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _familyService.AddOrUpdateAsync(new List<Family> { request }));
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepository()
        {
            // Arrange
            var family = new Family { FamilyId = 1, FamilyName = "New Family", ParishId = 1, UnitId = 1 };
            _mockFamilyRepository.Setup(repo => repo.AddAsync(family))
                .ReturnsAsync(family);

            // Act
            var result = await _familyService.AddAsync(family);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Family", result.FamilyName);
            _mockFamilyRepository.Verify(repo => repo.AddAsync(family), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepository()
        {
            // Arrange
            var family = new Family { FamilyId = 1, FamilyName = "Updated Family", ParishId = 1, UnitId = 1, JoinDate = DateTime.UtcNow };
            _mockFamilyRepository.Setup(repo => repo.UpdateAsync(family))
                .ReturnsAsync(family);

            // Act
            var result = await _familyService.UpdateAsync(family);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Family", result.FamilyName);
            _mockFamilyRepository.Verify(repo => repo.UpdateAsync(family), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldSetJoinDateToUtc()
        {
            // Arrange
            var localJoinDate = DateTime.Now;
            var family = new Family { FamilyId = 1, FamilyName = "Updated Family", ParishId = 1, UnitId = 1, JoinDate = localJoinDate };

            _mockFamilyRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Family>()))
                .ReturnsAsync((Family f) => f);

            // Act
            var result = await _familyService.UpdateAsync(family);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DateTimeKind.Utc, result.JoinDate?.Kind);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepository()
        {
            // Arrange
            var familyId = 1;
            _mockFamilyRepository.Setup(repo => repo.DeleteAsync(familyId))
                .Returns(Task.CompletedTask);

            // Act
            await _familyService.DeleteAsync(familyId);

            // Assert
            _mockFamilyRepository.Verify(repo => repo.DeleteAsync(familyId), Times.Once);
        }
    }

}