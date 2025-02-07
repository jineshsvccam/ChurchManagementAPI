using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using ChurchServices;
using Moq;
using Xunit;

namespace DistrictsTest
{
    public class DistrictServiceTests
    {
        private readonly Mock<IDistrictRepository> _mockDistrictRepository;
        private readonly DistrictService _districtService;

        public DistrictServiceTests()
        {
            _mockDistrictRepository = new Mock<IDistrictRepository>();
            _districtService = new DistrictService(_mockDistrictRepository.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDistricts()
        {
            // Arrange
            var districts = new List<District>
            {
                new District { DistrictId = 1, DistrictName = "District 1" },
                new District { DistrictId = 2, DistrictName = "District 2" }
            };
            _mockDistrictRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(districts);

            // Act
            var result = await _districtService.GetAllAsync();

            // Assert
            Assert.Equal(districts, result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDistrictById()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "District 1" };
            _mockDistrictRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(district);

            // Act
            var result = await _districtService.GetByIdAsync(1);

            // Assert
            Assert.Equal(district, result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddDistrict()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "District 1" };
            _mockDistrictRepository.Setup(repo => repo.AddAsync(district)).Returns(Task.CompletedTask);

            // Act
            await _districtService.AddAsync(district);

            // Assert
            _mockDistrictRepository.Verify(repo => repo.AddAsync(district), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateDistrict()
        {
            // Arrange
            var district = new District { DistrictId = 1, DistrictName = "District 1" };
            _mockDistrictRepository.Setup(repo => repo.UpdateAsync(district)).Returns(Task.CompletedTask);

            // Act
            await _districtService.UpdateAsync(district);

            // Assert
            _mockDistrictRepository.Verify(repo => repo.UpdateAsync(district), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteDistrictById()
        {
            // Arrange
            _mockDistrictRepository.Setup(repo => repo.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            await _districtService.DeleteAsync(1);

            // Assert
            _mockDistrictRepository.Verify(repo => repo.DeleteAsync(1), Times.Once);
        }
    }
}
