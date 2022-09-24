
using Moq;

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
        //_unitOfWork.Setup(r => r.AccountRepository).Returns(accountRepositoryMock.Object);//not necessary
        //_unitOfWork.Setup(r => r.MovementRepository).Returns(movementRepositorMock.Object);//not necessary
        _logger = new Mock<ILogger<TransferUseCase>>();
        _transferUseCase = new TransferUseCase(_logger.Object, _unitOfWork.Object);
        
        Setup();       
    }
    #endregion

    #region Setup
    private void Setup()
    {
        _transfer = new Transfer { Id = 1, FromAccountId = 1, ToAccountId = 2, Amount = 100, UserId = 1, CreatedAt = DateTime.Now };
        
        _account1 = new Account { Id = 1, UserId = 1, Balance = 900, Currency = Currency.EUR, CreatedAt = DateTime.Now };
        _account2 = new Account { Id = 2, UserId = 2, Balance = 900, Currency = Currency.EUR, CreatedAt = DateTime.Now };

        //var userId = 1;
        //_unitOfWork.Setup(r => r.AccountRepository.Create(_account1)).Returns(CreateMockOK(_account1));
        //_unitOfWork.Setup(r => r.AccountRepository.Create(_account2)).Returns(CreateMockOK(_account2));

        _unitOfWork.Setup(r => r.AccountRepository.ReadById(_account1.UserId, _account1.Id)).ReturnsAsync(() => (_account1));
        _unitOfWork.Setup(r => r.AccountRepository.ReadById(_account2.Id)).ReturnsAsync(() =>  (_account2));

        _unitOfWork.Setup(r => r.AccountRepository.Update(It.IsAny<Account>())).ReturnsAsync(() => (true));
        _unitOfWork.Setup(r => r.AccountRepository.Update(It.IsAny<Account>())).ReturnsAsync(() => (true));

        _movement1 = new Movement { Id = 1, AccountId = _transfer.FromAccountId, Amount = _transfer.Amount * (-1), Balance = _account1.Balance - _transfer.Amount, CreatedAt = DateTime.Now, UserId = 1 };
        _movement2 = new Movement { Id = 2, AccountId = _transfer.ToAccountId, Amount = _transfer.Amount, Balance = _account2.Balance + _transfer.Amount, CreatedAt = DateTime.Now, UserId = 1 };
        _unitOfWork.Setup(r => r.MovementRepository.Create(It.IsAny<Movement>())).ReturnsAsync(() => (true, 1));
        _unitOfWork.Setup(r => r.MovementRepository.Create(It.IsAny<Movement>())).ReturnsAsync(() => (true, 2));
    }
    /*
    private async Task<(bool, int?)> CreateMockOK(Movement movement)
    {
        return (true, movement.Id);
    }
    /*
    private async Task<(bool, int?)> CreateMockOK(Account account)
    {
        return (true, account.Id);
    }
    */
    /*
    private async Task<bool> UpdateMockOK()
    {
        return true;
    }
    */
    /*
    private async Task<Account?> ReadByIdMockOk(int id)
    {
        return id == 1 ?  _account1 :_account2;
    }
    */
    #endregion

    #region Tests
    [Fact]
    public async Task Transfer_TestOK()
    {
        // Arrange
        // Act
        var result = await _transferUseCase.Transfer(_transfer);
        // Assert
        Assert.Null(result.Item1);
    }

    [Fact]
    // Account not found.
    public async Task Transfer_TestUserAccountNotFoundError()
    {
        // Arrange
        _transfer.UserId = 2;
        // Act
        var result = await _transferUseCase.Transfer(_transfer);
        // Assert
        Assert.NotNull(result.Item1);
        Assert.Equal(EnumHelper.GetEnumDescription(ErrorUsecase.TransferAccountNotFound), result.Item2, ignoreCase: true);
    }

    [Fact]
    //"Account with different currencies."
    public async Task Transfer_TestCurrencyAccountError()
    {
        // Arrange
        _account2.Currency = Currency.USD;
        // Act
        var result = await _transferUseCase.Transfer(_transfer);
        // Assert
        Assert.NotNull(result.Item1);
        Assert.Equal(EnumHelper.GetEnumDescription(ErrorUsecase.TransferDifferentCurrencies), result.Item2, ignoreCase: true);
    }

    [Fact]
    //Balance below amount.
    public async Task Transfer_TestBalanceAccountError()
    {
        // Arrange
        _account1.Balance = _transfer.Amount - 1;
        // Act
        var result = await _transferUseCase.Transfer(_transfer);
        // Assert
        Assert.NotNull(result.Item1);
        Assert.Equal(EnumHelper.GetEnumDescription(ErrorUsecase.TransferBalanceBelowAmount), result.Item2, ignoreCase: true);
    }

    [Fact]
    //The accounts are the same.
    public async Task Transfer_TestSameAccountsError()
    {
        // Arrange
        _transfer.ToAccountId = _transfer.FromAccountId;
        // Act
        var result = await _transferUseCase.Transfer(_transfer);
        // Assert
        Assert.NotNull(result.Item1);
        Assert.Equal(EnumHelper.GetEnumDescription(ErrorUsecase.TransferSameAccount), result.Item2, ignoreCase: true);
    }
    
    [Fact]
    //Gerar exception (internal server error)
    public async Task Transfer_TestError()
    {
        // Arrange
        _unitOfWork.Setup(r => r.AccountRepository.Update(_account2)).Throws(new Exception());
        // Act
        var result = await _transferUseCase.Transfer(_transfer);
        // Assert
        Assert.NotNull(result.Item1);
        Assert.Equal(EnumHelper.GetEnumDescription(ErrorUsecase.TransferError), result.Item2, ignoreCase: true);
    }
    #endregion
}