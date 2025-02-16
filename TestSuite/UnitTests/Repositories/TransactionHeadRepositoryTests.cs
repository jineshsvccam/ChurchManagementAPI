using ChurchCommon.Utils;
using ChurchData;
using ChurchRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
namespace TransacionHeadsTest
{
    public class TransactionHeadRepositoryTests
    {
        private readonly Mock<ApplicationDbContext> _mockDbContext;
        private readonly Mock<IOptions<LoggingSettings>> _mockLoggingSettings;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ILogger<TransactionHeadRepository>> _mockLogger;
        private readonly Mock<LogsHelper> _mockLogsHelper;
        private readonly TransactionHeadRepository _transactionHeadRepository;

        public TransactionHeadRepositoryTests()
        {
            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockLoggingSettings = new Mock<IOptions<LoggingSettings>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockLogger = new Mock<ILogger<TransactionHeadRepository>>();
            _mockLogsHelper = new Mock<LogsHelper>();

            // Mock UserManager requires a specific constructor setup
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

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
                _mockLoggingSettings.Object,
                _mockUserManager.Object,
                _mockHttpContextAccessor.Object,
                _mockLogger.Object,
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
