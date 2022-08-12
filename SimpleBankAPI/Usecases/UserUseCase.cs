using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Usecases;

public class UserUseCase
{
    private readonly ILogger<UserUseCase> _logger;
    private readonly IUserRepository _repository;
    public UserUseCase(ILogger<UserUseCase> logger, IUserRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
    public async Task<UserModel> CreateUser(UserModel user)
    {
        return await Task.FromResult(user);
    }

    public async Task<IEnumerable<UserModel>> GetUsers()
    {
        return await _repository.Read();
    }
}
