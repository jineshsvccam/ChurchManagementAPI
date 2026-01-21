using ChurchCommon.Utils;
using ChurchData;
using ChurchRepositories.Settings;
using Microsoft.AspNetCore.Http;
using Moq;

namespace TransacionHeadsTest
{
    public class TransactionHeadRepositoryTests
    {
        private readonly Mock<ApplicationDbContext> _mockDbContext;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<LogsHelper> _mockLogsHelper;
        private readonly TransactionHeadRepository _transactionHeadRepository;

        public TransactionHeadRepositoryTests()
        {
            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockLogsHelper = new Mock<LogsHelper>();

            // Mock TransactionHeads DbSet
            var transactionHeads = new List<TransactionHead>
            {
                new TransactionHead { HeadId = 1, HeadName = "Head 1" },
                new TransactionHead { HeadId = 2, HeadName = "Head 2" }
            };
            var mockTransactionHeadDbSet = DbSetMockHelper.CreateMockDbSet(transactionHeads);
            _mockDbContext.Setup(db => db.TransactionHeads).Returns(mockTransactionHeadDbSet.Object);

            // Pass all mocked dependencies into TransactionHeadRepository
            _transactionHeadRepository = new TransactionHeadRepository(
                _mockDbContext.Object,
                _mockHttpContextAccessor.Object,
                _mockLogsHelper.Object
            );
        }

        [Fact]
        public async Task GetTransactionHeadsAsync_ShouldReturnData()
        {
            // Act
            var result = await _transactionHeadRepository.GetTransactionHeadsAsync(null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
    }
}
