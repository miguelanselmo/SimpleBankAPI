using Microsoft.IdentityModel.Tokens;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Providers;
using SimpleBankAPI.Infrastructure.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleBankAPI.Core.Usecases;

public class UserUseCase : IUserUseCase
{
    private readonly ILogger<UserUseCase> logger;
    private readonly IAuthenticationProvider provider;
    private readonly IUnitOfWork unitOfWork;


    public UserUseCase(ILogger<UserUseCase> logger, IAuthenticationProvider provider, IUnitOfWork unitOfWork)
    {
        this.logger = logger;
        this.provider = provider;
        this.unitOfWork = unitOfWork;
    }

    public async Task<(bool,string?, UserModel?)> CreateUser(UserModel user)
    {
        var userDb = await unitOfWork.UserRepository.ReadByName(user.UserName);
        if (userDb is null)
        {
            var result = await unitOfWork.UserRepository.Create(user);
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
        var userDb = await unitOfWork.UserRepository.ReadByName(user.UserName);
        if (userDb is not null)
        {
            if (userDb.Password.Equals(user.Password))
            {
                return (true, null, userDb, provider.GenerateToken(userDb));
            }
            else
                return (false, "Invalid authentication", null, null);
        }
        else
            return (false, "User not found", null, null);
    }
}
