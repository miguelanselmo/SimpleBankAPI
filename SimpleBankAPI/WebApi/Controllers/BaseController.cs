using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Providers;
using SimpleBankAPI.Infrastructure.Repositories;
using SimpleBankAPI.Core.Usecases;
using SimpleBankAPI.WebApi.Models;

namespace SimpleBankAPI.WebApi.Controllers;

public abstract class BaseController<T> : Controller
{
    /*
    private readonly ILogger<TransferController> _logger;
    private readonly IAuthenticationProvider _provider;
    
        public BaseController(ILogger<TransferController> logger, IAuthenticationProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }
    */
    public async Task<(bool, string?, Session?)> GetToken(string token, IAuthenticationProvider _provider)
    {
        if (!Request.Headers.TryGetValue("Authorization", out StringValues authToken))
            return (false, "Missing Authorization Header.", null);
        var resultAuth = _provider.GetToken(authToken);
        if (!resultAuth.Item1)
            return (false, "Missing User info in Token.", null);
        var resultClaimsUser = _provider.GetClaimUser(resultAuth.Item2);
        if (!resultClaimsUser.Item1)
            return (false, "Missing User info in Token.", null);
        var resultClaimsSession = _provider.GetClaimSession(resultAuth.Item2);
        if (!resultClaimsSession.Item1)
            return (false, "Missing Session info in Token.", null);
        return (true, null, new Session
        {
            TokenAccess = token,
            //TokenAccessExpireAt
            //TokenRefresh
            //TokenRefreshExpireAt
            Id = resultClaimsSession.Item2.Id,
            UserId = resultClaimsUser.Item2.Id,
            //CreatedAt
            Active = true
        });
    }
}
