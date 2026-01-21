using ChurchCommon.Utils;
using ChurchData;
using ChurchRepositories.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
namespace FamilyTests
{
    public class FamilyRepositoryTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<ILogger<FamilyRepository>> _mockLogger;
        private readonly Mock<LogsHelper> _mockLogsHelper;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly FamilyRepository _familyRepository;

        public FamilyRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test run
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<FamilyRepository>>();
            _mockLogsHelper = new Mock<LogsHelper>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            _familyRepository = new FamilyRepository(_dbContext, _mockLogger.Object, _mockLogsHelper.Object, _mockHttpContextAccessor.Object);
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
            await _dbContext.Families.AddRangeAsync(families);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _familyRepository.GetFamiliesAsync(null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnFamily_WhenExists()
        {
            // Arrange
            var family = new Family { FamilyId = 1, FamilyName = "John Family", ParishId = 1, UnitId = 1 };
            await _dbContext.Families.AddAsync(family);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _familyRepository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Family", result.FamilyName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _familyRepository.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddFamily()
        {
            // Arrange
            var family = new Family { FamilyId = 1, FamilyName = "New Family", ParishId = 1, UnitId = 1 };

            // Act
            var result = await _familyRepository.AddAsync(family);
            var savedFamily = await _dbContext.Families.FindAsync(1);

            // Assert
            Assert.NotNull(savedFamily);
            Assert.Equal("New Family", savedFamily.FamilyName);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingFamily()
        {
            // Arrange
            var family = new Family { FamilyId = 1, FamilyName = "Old Name", ParishId = 1, UnitId = 1 };
            await _dbContext.Families.AddAsync(family);
            await _dbContext.SaveChangesAsync();

            var updatedFamily = new Family { FamilyId = 1, FamilyName = "Updated Name", ParishId = 1, UnitId = 1 };

            // Act
            var result = await _familyRepository.UpdateAsync(updatedFamily);
            var savedFamily = await _dbContext.Families.FindAsync(1);

            // Assert
            Assert.NotNull(savedFamily);
            Assert.Equal("Updated Name", savedFamily.FamilyName);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenFamilyNotFound()
        {
            // Arrange
            var nonExistentFamily = new Family { FamilyId = 99, FamilyName = "Non-existent Family", ParishId = 1, UnitId = 1 };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _familyRepository.UpdateAsync(nonExistentFamily));
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteFamily_WhenExists()
        {
            // Arrange
            var family = new Family { FamilyId = 1, FamilyName = "To Delete", ParishId = 1, UnitId = 1 };
            await _dbContext.Families.AddAsync(family);
            await _dbContext.SaveChangesAsync();

            // Act
            await _familyRepository.DeleteAsync(1);
            var deletedFamily = await _dbContext.Families.FindAsync(1);

            // Assert
            Assert.Null(deletedFamily);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenFamilyNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _familyRepository.DeleteAsync(99));
        }
    }
}
