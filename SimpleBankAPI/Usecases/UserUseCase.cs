using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Usecases;

public class UserUseCase : IUserUseCase
{
    private readonly ILogger<UserUseCase> _logger;
    private readonly IUserRepository _repository;
    public UserUseCase(ILogger<UserUseCase> logger, IUserRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
    public async Task<(bool,string?, UserModel?)> CreateUser(UserModel user)
    {
        var userDb = await _repository.ReadByUsername(user.UserName);
        if (userDb is null)
        {
            var result = await _repository.Create(user);
            if (result.Item1)
            {
                user.Id = (int)result.Item2;
                return (true, null, user);
            }
            else
                return (false, "User not created. Please try again.", null);
        }
        else
            return (false, "Username already exists", null);
    }
    
    public async Task<(bool,string?)> Login(UserModel user)
    {
        var userDb = await _repository.ReadByUsername(user.UserName);
        if (userDb is not null)
        {
            if (userDb.Password.Equals(user.Password))
                return (true, null);
            else
                return (false, "Invalid authentication");
        }
        else
        {
            return (false, "User not found");
        }
    }
    
}
