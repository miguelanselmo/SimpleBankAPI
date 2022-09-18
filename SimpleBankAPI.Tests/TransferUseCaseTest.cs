
namespace SimpleBankAPI.Tests;

public class TransferUseCaseTest
{
    #region Members
    
    private readonly ITransferUseCase _transferUseCase;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<TransferUseCase>> _logger;

    private Transfer _transfer;
    private Account _account1;
    private Account _account2;
    private Movement _movement1;
    private Movement _movement2;

    #endregion

    #region Constructor
    public TransferUseCaseTest()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var accountRepositoryMock = new Mock<IAccountRepository>();
        var movementRepositorMock = new Mock<IMovementRepository>();
        var sessionRepositoryMock = new Mock<ISessionRepository>();

        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.Setup(r => r.AccountRepository).Returns(accountRepositoryMock.Object);
        _unitOfWork.Setup(r => r.MovementRepository).Returns(movementRepositorMock.Object);
        _logger = new Mock<ILogger<TransferUseCase>>();
        _transferUseCase = new TransferUseCase(_logger.Object, _unitOfWork.Object);

        Setup();       
    }
    #endregion

    #region Setup
    public void Setup()
    {
        _account1 = new Account { Id = 1, UserId = 1, Balance = 900, Currency = Currency.EUR, CreatedAt = DateTime.Now };
        _account2 = new Account { Id = 2, UserId = 1, Balance = 900, Currency = Currency.EUR, CreatedAt = DateTime.Now };

        var userId = 1;
        _unitOfWork.Setup(r => r.AccountRepository.Create(_account1)).Returns(CreateMockOK(_account1));
        _unitOfWork.Setup(r => r.AccountRepository.Create(_account2)).Returns(CreateMockOK(_account2));

        _unitOfWork.Setup(r => r.AccountRepository.ReadById(userId, 1)).Returns(ReadByIdMockOk(1));
        _unitOfWork.Setup(r => r.AccountRepository.ReadById(2)).Returns(ReadByIdMockOk(2));

        _unitOfWork.Setup(r => r.AccountRepository.Update(_account1)).Returns(UpdateMockOK());
        _unitOfWork.Setup(r => r.AccountRepository.Update(_account2)).Returns(UpdateMockOK());

        _movement1 = new Movement { AccountId = 1, Amount = -100, Balance = 800 };
        _movement2 = new Movement { AccountId = 2, Amount = 100, Balance = 1000 };
        _unitOfWork.Setup(r => r.MovementRepository.Create(_movement1)).Returns(CreateMockOK(_movement1));
        _unitOfWork.Setup(r => r.MovementRepository.Create(_movement2)).Returns(CreateMockOK(_movement2));

        _transfer = new Transfer { Id = 1, FromAccountId = 1, ToAccountId = 2, Amount = 100, UserId = 1, CreatedAt = DateTime.Now };
    }

    private async Task<(bool, int?)> CreateMockOK(Movement movement)
    {
        return (true, movement.Id);
    }

    private async Task<(bool, int?)> CreateMockOK(Account account)
    {
        return (true, account.Id);
    }

    private async Task<bool> UpdateMockOK()
    {
        return true;
    }

    private async Task<Account?> ReadByIdMockOk(int id)
    {
        return id == 1 ?  _account1 :_account2;
    }
    #endregion

    #region Tests
    [Fact]
    public async Task Transfer_TestOK()
    {
        // Arrange
        // Act
        var result = await _transferUseCase.Transfer(_transfer);
        // Assert
        Assert.True(result.Item1);
    }
    #endregion
}