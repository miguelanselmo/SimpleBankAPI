using SimpleBankAPI.Application.Interfaces;
using SimpleBankAPI.Infrastructure.Crypto;
using SimpleBankAPI.Infrastructure.Ports.Repositories;

namespace SimpleBankAPI.Application.Usecases;

public class UserUseCase : IUserUseCase
{
    private readonly ILogger<UserUseCase> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UserUseCase(ILogger<UserUseCase> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool, string?, User?)> CreateUser(User user)
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating user");
            return (false, "Error creating user", null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }

}
