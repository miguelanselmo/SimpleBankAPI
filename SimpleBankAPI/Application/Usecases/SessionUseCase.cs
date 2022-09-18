using SimpleBankAPI.Application.Interfaces;
using SimpleBankAPI.Infrastructure.Crypto;
using SimpleBankAPI.Infrastructure.Ports.Providers;
using SimpleBankAPI.Infrastructure.Ports.Repositories;

namespace SimpleBankAPI.Application.Usecases;

public class SessionUseCase : ISessionUseCase
{
    private readonly ILogger<SessionUseCase> _logger;
    private readonly IAuthenticationProvider _provider;
    private readonly IUnitOfWork _unitOfWork;

    public SessionUseCase(ILogger<SessionUseCase> logger, IAuthenticationProvider provider, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _provider = provider;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool, string?, User?, Session?)> Login(User user)
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
                    return result ? (true, null, userDb, session) : (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionCreateError), null, null);
                }
                else
                    return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionInvalidAuthentication), null, null);
            }
            else
                return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionUserNotFound), null, null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.SessionCreateError));
            return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionCreateError), null, null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }

    public async Task<(bool, string?, User?, Session?)> RenewLogin(Session session, string refreshToken)
    {
        bool commit = false;
        try
        {
            //var sessionDb = await _unitOfWork.SessionRepository.ReadById(session.Id);
            //if (sessionDb is not null)
            //{
            //if (sessionDb.Active)
            //{
            if (session.TokenRefresh == refreshToken && session.TokenRefreshExpireAt > DateTime.UtcNow)
            {
                var userDb = await _unitOfWork.UserRepository.ReadById(session.UserId);
                if (userDb is not null)
                {
                    session = _provider.RenewToken(userDb, session);
                    return (true, null, userDb, session);
                }
                else
                    return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionUserNotFound), null, null);
            }
            return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionTokenRefreshInvalid), null, null);
            //}
            //else
            //    return (false, "Session already closed", null, null);
            //}
            //else
            //    return (false, "Session not found", null, null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.SessionRenewError));
            return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionRenewError), null, null);
        }
        finally
        {
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
                    return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionAlreadyClosed), null);
            }
            else
                return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionNotFound), null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.SessionCloseError));
            return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionCloseError), null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }

    public async Task<(bool, string?, Session?)> CheckSession(Session session)
    {
        try
        {
            var sessionDb = await _unitOfWork.SessionRepository.ReadById(session.Id);
            if (sessionDb is null)
                return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionNotFound), null);
            if (!sessionDb.Active)
                return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionAlreadyClosed), sessionDb);
            return (true, null, sessionDb);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.SessionCheckError));
            return (false, EnumHelper.GetEnumDescription(ErrorUsecase.SessionCheckError), null);
        }
    }

}
