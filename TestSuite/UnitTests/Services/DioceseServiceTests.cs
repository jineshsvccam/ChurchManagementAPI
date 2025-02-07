using Xunit;
using Moq;
using ChurchContracts;
using ChurchServices;
using ChurchData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiocesesTest
{
    public class DioceseServiceTests
    {
        private readonly Mock<IDioceseRepository> _dioceseRepositoryMock;
        private readonly DioceseService _dioceseService;

        public DioceseServiceTests()
        {
            _dioceseRepositoryMock = new Mock<IDioceseRepository>();
            _dioceseService = new DioceseService(_dioceseRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfDioceses_WhenDiocesesExist()
        {
            // Arrange
            var dioceses = new List<Diocese>
        {
            new Diocese { DioceseId = 1, DioceseName = "Diocese A" },
            new Diocese { DioceseId = 2, DioceseName = "Diocese B" }
        };
            _dioceseRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(dioceses);

            // Act
            var result = await _dioceseService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Diocese A", result.First().DioceseName);
            Assert.Equal("Diocese B", result.Last().DioceseName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDiocese_WhenDioceseExists()
        {
            // Arrange
            var dioceseId = 1;
            var diocese = new Diocese { DioceseId = dioceseId, DioceseName = "Diocese A" };
            _dioceseRepositoryMock.Setup(repo => repo.GetByIdAsync(dioceseId)).ReturnsAsync(diocese);

            // Act
            var result = await _dioceseService.GetByIdAsync(dioceseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dioceseId, result.DioceseId);
        }

        [Fact]
        public async Task AddAsync_ShouldInvokeAddOnRepository_WhenValidDioceseIsProvided()
        {
            // Arrange
            var diocese = new Diocese { DioceseId = 1, DioceseName = "Diocese A" };

            // Act
            await _dioceseService.AddAsync(diocese);

            // Assert
            _dioceseRepositoryMock.Verify(repo => repo.AddAsync(diocese), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldInvokeUpdateOnRepository_WhenValidDioceseIsProvided()
        {
            // Arrange
            var diocese = new Diocese { DioceseId = 1, DioceseName = "Updated Diocese A" };

            // Act
            await _dioceseService.UpdateAsync(diocese);

            // Assert
            _dioceseRepositoryMock.Verify(repo => repo.UpdateAsync(diocese), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldInvokeDeleteOnRepository_WhenValidIdIsProvided()
        {
            // Arrange
            var dioceseId = 1;

            // Act
            await _dioceseService.DeleteAsync(dioceseId);

            // Assert
            _dioceseRepositoryMock.Verify(repo => repo.DeleteAsync(dioceseId), Times.Once);
        }
    }
}
