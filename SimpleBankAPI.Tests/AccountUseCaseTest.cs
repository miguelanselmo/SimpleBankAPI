
using System.Collections.Generic;

namespace SimpleBankAPI.Tests;

public class AccountUseCaseTest
{
    #region Members
    private readonly IAccountUseCase _accountUseCase;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<AccountUseCase>> _logger;

    private Account _account;
    private Movement _movement;
    #endregion

    #region Constructor
    public AccountUseCaseTest()
    {
        var userRepositoryMock  = new Mock<IUserRepository>();
        var accountRepositoryMock = new Mock<IAccountRepository>();
        var movementRepositorMock = new Mock<IMovementRepository>();
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        
        _unitOfWork = new Mock<IUnitOfWork>();
        //_unitOfWork.Setup(r => r.AccountRepository).Returns(accountRepositoryMock.Object);
        _logger = new Mock<ILogger<AccountUseCase>>();
        _accountUseCase = new AccountUseCase(_logger.Object, _unitOfWork.Object);

        Setup();
    }
    #endregion

    #region Setup
    public void Setup()
    {
        _account = new Account { Id = 1, UserId = 1, Balance = 1000, Currency = Currency.EUR, CreatedAt = DateTime.Now };
        _movement = new Movement { Id = 1, AccountId = 1, Amount = 1000, Balance = 900, CreatedAt = DateTime.Now, UserId = 1 };
        
        _unitOfWork.Setup(r => r.AccountRepository.Create(_account)).Returns(CreateMockOK(_account));

        var id = 1;
        var userId = 1;
        _unitOfWork.Setup(r => r.AccountRepository.ReadById(userId, id)).Returns(ReadByIdMockOk());
        _unitOfWork.Setup(r => r.MovementRepository.ReadByAccount(id)).Returns(ReadByAccountMockOk());
    }

    private async Task<(bool, int?)> CreateMockOK(Account account)
    {
        return (true, account.Id);
    }

    private async Task<Account?> ReadByIdMockOk()
    {
        return _account;
    }

    private async Task<IEnumerable<Movement>?> ReadByAccountMockOk()
    {
        var movements = new List<Movement>();
        movements.Add(_movement);
        return movements;
    }
    #endregion

    #region Tests
    [Fact]
    public async Task CreateAccount_TestOK()
    {
        // Arrange
        // Act
        var result = await _accountUseCase.CreateAccount(_account);        
        // Assert
        Assert.True(result.Item1);
    }


    [Fact]
    public async Task CreateAccount_TestError()
    {
        // Arrange
        _unitOfWork.Setup(r => r.AccountRepository.Create(_account)).Throws(new Exception());
        // Act
        var result = await _accountUseCase.CreateAccount(_account);        
        // Assert
        Assert.False(result.Item1);
    }

    [Fact]
    public async Task GetAllAccounts_TestOK()
    {
        // Arrange
        var userId = 1;
        // Act
        var result = await _accountUseCase.GetAccounts(userId);
        // Assert
        Assert.True(result.Item1);
    }

    [Fact]
    public async Task GetAllAccounts_TestError()
    {
        // Arrange
        var userId = 1;
        _unitOfWork.Setup(r => r.AccountRepository.ReadByUser(userId)).Throws(new Exception());
        // Act
        var result = await _accountUseCase.GetAccounts(userId);
        // Assert
        Assert.False(result.Item1);
    }        

    [Fact]
    public async Task GetAccountById_TestOK()
    {
        // Arrange
        var id = 1;
        var userId = 1;   
        // Act
        var result = await _accountUseCase.GetAccountMovements(userId, id);
        // Assert
        Assert.True(result.Item1);
    }

    [Fact]
    public async Task GetAccountById_TestNotFound()
    {
        // Arrange
        var id = 2;
        var userId = 1;
        // Act
        var result = await _accountUseCase.GetAccountMovements(userId, id);
        // Assert
        Assert.False(result.Item1);
    }

    [Fact]
    public async Task GetAccountById_TestError()
    {
        // Arrange
        var id = 1;
        var userId = 1;
        _unitOfWork.Setup(r => r.AccountRepository.ReadById(userId, id)).Throws(new Exception());
        // Act
        var result = await _accountUseCase.GetAccountMovements(userId, id);
        // Assert
        Assert.False(result.Item1);
    }
    #endregion
}