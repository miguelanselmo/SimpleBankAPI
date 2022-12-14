
using Moq;
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
        //_unitOfWork.Setup(r => r.AccountRepository).Returns(accountRepositoryMock.Object);//not necessary
        _logger = new Mock<ILogger<AccountUseCase>>();
        _accountUseCase = new AccountUseCase(_logger.Object, _unitOfWork.Object);

        Setup();
    }
    #endregion

    #region Setup
    private void Setup()
    {
        _account = new Account { Id = 1, UserId = 1, Balance = 1000, Currency = Currency.EUR, CreatedAt = DateTime.Now };
        _movement = new Movement { Id = 1, AccountId = 1, Amount = 1000, Balance = 900, CreatedAt = DateTime.Now, UserId = 1 };
        
        _unitOfWork.Setup(r => r.AccountRepository.Create(_account)).ReturnsAsync(() => (true, _account.Id));

        var id = 1;
        var userId = 1;
        _unitOfWork.Setup(r => r.AccountRepository.ReadById(userId, id)).ReturnsAsync(() => _account);
        _unitOfWork.Setup(r => r.MovementRepository.ReadByAccount(id)).Returns(ReadByAccountMockOk());
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
        Assert.Null(result.Item1);
    }


    [Fact]
    public async Task CreateAccount_TestError()
    {
        // Arrange
        _unitOfWork.Setup(r => r.AccountRepository.Create(_account)).Throws(new Exception());
        // Act
        var result = await _accountUseCase.CreateAccount(_account);        
        // Assert
        Assert.NotNull(result.Item1);
        Assert.Equal(EnumHelper.GetEnumDescription(ErrorUsecase.AccountCreatError), result.Item2);
    }

    [Fact]
    public async Task GetAllAccounts_TestOK()
    {
        // Arrange
        var userId = 1;
        // Act
        var result = await _accountUseCase.GetAccounts(userId);
        // Assert
        Assert.Null(result.Item1);
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
        Assert.NotNull(result.Item1);
        Assert.Equal(EnumHelper.GetEnumDescription(ErrorUsecase.AccountReadError), result.Item2);
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
        Assert.Null(result.Item1);
    }

    [Fact]
    //Conta inexistente(not found)
    public async Task GetAccountById_TestNotFound()
    {
        // Arrange
        var id = 2;
        var userId = 1;
        // Act
        var result = await _accountUseCase.GetAccountMovements(userId, id);
        // Assert
        Assert.NotNull(result.Item1);
        Assert.Equal(EnumHelper.GetEnumDescription(ErrorUsecase.AccountNotFound), result.Item2);
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
        Assert.NotNull(result.Item1);
        Assert.Equal(EnumHelper.GetEnumDescription(ErrorUsecase.AccountMovementReadError), result.Item2);
    }
    #endregion
}