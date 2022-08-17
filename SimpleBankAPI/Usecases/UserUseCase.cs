using Microsoft.IdentityModel.Tokens;
using SimpleBankAPI.Models;
using SimpleBankAPI.Providers;
using SimpleBankAPI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleBankAPI.Usecases;

public class UserUseCase : IUserUseCase
{
    private readonly ILogger<UserUseCase> logger;
    private readonly IUserRepository repository;
    private readonly IConfiguration config;
    private readonly IAuthenticationProvider provider;

    
    public UserUseCase(ILogger<UserUseCase> logger, IAuthenticationProvider provider, IUserRepository repository)
    {
        this.logger = logger;
        this.provider = provider;
        this.repository = repository;
    }

    public async Task<(bool,string?, UserModel?)> CreateUser(UserModel user)
    {
        var userDb = await repository.ReadById(user.Id);
        if (userDb is null)
        {
            var result = await repository.Create(user);
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
    
    public async Task<(bool,string?,UserModel?,SessionModel?)> Login(UserModel user)
    {
        var userDb = await repository.ReadById(user.Id);
        if (userDb is not null)
        {
            if (userDb.Password.Equals(user.Password))
            {
                var dataModel = provider.GenerateToken(user);
                return (true, null, userDb, provider.GenerateToken(user));
            }
            else
                return (false, "Invalid authentication", null, null);
        }
        else
            return (false, "User not found", null, null);
    }
}
