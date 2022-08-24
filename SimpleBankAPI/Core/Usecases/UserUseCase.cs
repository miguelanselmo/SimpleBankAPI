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
    private readonly IAuthenticationProvider _provider;
    private readonly IUnitOfWork _unitOfWork;


    public UserUseCase(ILogger<UserUseCase> logger, IAuthenticationProvider provider, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _provider = provider;
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
    
    public async Task<(bool,string?,User?,Session?)> Login(User user)
    {
        bool commit = false;
        try
        {
            var userDb = await _unitOfWork.UserRepository.ReadByName(user.UserName);
            if (userDb is not null)
            {
                if (Crypto.VerifySecret(userDb.Password, user.Password))
                {
                    Session session = _provider.GenerateToken(userDb);
                    session.Active = true;
                    session.UserId = userDb.Id;
                    var result = await _unitOfWork.SessionRepository.Create(session);
                    commit = result;
                    return result ? (true, null, userDb, session) : (false, "Error creating session", null, null);
                }
                else
                    return (false, "Invalid authentication", null, null);
            }
            else
                return (false, "User not found", null, null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }
    
    public async Task<(bool, string?, User?, Session?)> RenewLogin(Session session)
    {
        bool commit = false;
        try
        {
            var userDb = await _unitOfWork.UserRepository.ReadById(session.UserId);
            if (userDb is not null)
            {
                session = _provider.RenewToken(session, userDb);
                //session.Active = true;
                //session.UserId = userDb.Id;
                //var result = await _unitOfWork.SessionRepository.Create(session);
                //commit = result;
                //return result ? (true, null, userDb, session) : (false, "Error creating session", null, null);
                return (true, null, userDb, session);
            }
            else
                return (false, "User not found", null, null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }
    
    public async Task<(bool, string?, Session?)> Logout(Session session)
    {
        bool commit = false;
        try
        {
            var sessionDb = await _unitOfWork.SessionRepository.ReadById(session.Id);
            if (sessionDb is not null)
            {
                if (sessionDb.Active)
                {
                    sessionDb.Active = false;
                    var result = await _unitOfWork.SessionRepository.Update(sessionDb);
                    commit = result;
                    return (true, null, sessionDb); ;
                }
                else
                    return (false, "Session already closed", null);
            }
            else
                return (false, "Session not found", null);
        }        
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }

    public async Task<(bool, string?, Session?)> CheckSession(Session session)
    {
        var sessionDb = await _unitOfWork.SessionRepository.ReadById(session.Id);
        if (sessionDb is null)
            return (false, "Session not found", null);
        if (!sessionDb.Active)
            return (false, "Session is closed", sessionDb);
        return (true, null, sessionDb);
    }

}
