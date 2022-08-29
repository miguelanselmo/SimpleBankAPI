using Microsoft.IdentityModel.Tokens;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Crypto;
using SimpleBankAPI.Infrastructure.Providers;
using SimpleBankAPI.Infrastructure.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleBankAPI.Core.Usecases;

internal class UserUseCase : IUserUseCase
{
    private readonly ILogger<UserUseCase> _logger;
    private readonly IUnitOfWork _unitOfWork;


    public UserUseCase(ILogger<UserUseCase> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<(bool,string?, User?)> CreateUser(User user)
    {
        bool commit = false;
        try
        {
            _unitOfWork.Begin();
            var userDb = await _unitOfWork.UserRepository.ReadByName(user.UserName);
            if (userDb is null)
            {
                user.Password = Crypto.HashSecret(user.Password);
                var result = await _unitOfWork.UserRepository.Create(user);
                if (result.Item1)
                {
                    commit = true;
                    user.Id = (int)result.Item2;
                    return (true, null, user);
                }
                else
                    return (false, "User not created. Please try again.", null);
            }
            else
                return (false, "Username already exists", null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }

}
