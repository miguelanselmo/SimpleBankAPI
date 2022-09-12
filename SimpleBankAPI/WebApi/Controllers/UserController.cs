using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.WebApi.Models;
using SimpleBankAPI.Application.Interfaces;
using SimpleBankAPI.Infrastructure.Ports.Providers;

namespace SimpleBankAPI.WebApi.Controllers;

[Route("simplebankapi/v1/[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserUseCase _useCase;
    private readonly ISessionUseCase _sessionUseCase;
    private readonly IAuthenticationProvider _provider;

    public UserController(ILogger<UserController> logger, IUserUseCase useCase, ISessionUseCase sessionUseCase, IAuthenticationProvider provider)
    {
        _logger = logger;
        _useCase = useCase;
        _sessionUseCase = sessionUseCase;
        _provider = provider;
    }


    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost(Name = "Register")]
    [ProducesResponseType(typeof(registerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<registerResponse>> Post([FromBody] registerRequest request)
    {
        try
        {
            var account = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                Password = request.Password,
                FullName = request.FullName
            };
            var result = await _useCase.CreateUser(account);
            if (result.Item1)
            {
                return Ok(new registerResponse
                {
                    UserId = result.Item3.Id,
                    UserName = result.Item3.UserName,
                    Email = result.Item3.Email,
                    FullName = result.Item3.FullName
                });
            }
            else
                return BadRequest(result.Item2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("Login", Name = "Login")]
    [ProducesResponseType(typeof(loginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<loginResponse>> Login([FromBody] loginRequest request)
    {
        try
        {
            User account = new User
            {
                UserName = request.UserName,
                Password = request.Password
            };
            var result = await _sessionUseCase.Login(account);
            if (result.Item1)
            {
                loginResponse response = new loginResponse
                {
                    AccessToken = result.Item4.TokenAccess,
                    AccessTokenExpiresAt = result.Item4.TokenAccessExpireAt,
                    RefreshToken = result.Item4.TokenRefresh,
                    RefreshTokenExpiresAt = result.Item4.TokenRefreshExpireAt,
                    SessionId = result.Item4.Id.ToString(),
                    User = new registerResponse
                    {
                        UserId = result.Item3.Id,
                        UserName = result.Item3.UserName,
                        Email = result.Item3.Email,
                        FullName = result.Item3.FullName,
                        CreatedAt = result.Item3.CreatedAt
                    }
                };
                return Ok(response);
            }
            else
                return Unauthorized(result.Item2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("Logout", Name = "Logout")]
    [ProducesResponseType(typeof(loginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<loginResponse>> Logout()
    {
        try
        {
            if (!Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                return BadRequest("Missing Authorization Header.");
            var resultClaims = _provider.GetClaims(authToken);
            if (!resultClaims.Item1)
                return BadRequest(resultClaims.Item2);
            var result = await _sessionUseCase.Logout(resultClaims.Item3);
            if (result.Item1)
                return Ok(result.Item3);
            else
                return BadRequest(result.Item2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("RenewLogin", Name = "RenewLogin")]
    [ProducesResponseType(typeof(loginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<loginResponse>> RenewLogin([FromBody] renewloginRequest tokenRefresh)
    {
        try
        {
            if (!Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                return BadRequest("Missing Authorization Header.");
            var resultClaims = _provider.GetClaims(authToken);
            if (!resultClaims.Item1)
                return BadRequest(resultClaims.Item2);
            var resultSession = await _sessionUseCase.CheckSession(resultClaims.Item3);
            if (!resultSession.Item1)
                return Unauthorized(resultSession.Item2);
            var result = await _sessionUseCase.RenewLogin(resultSession.Item3, tokenRefresh.RefreshToken);
            if (result.Item1)
            {
                loginResponse response = new loginResponse
                {
                    AccessToken = result.Item4.TokenAccess,
                    AccessTokenExpiresAt = result.Item4.TokenAccessExpireAt,
                    RefreshToken = result.Item4.TokenRefresh,
                    RefreshTokenExpiresAt = result.Item4.TokenRefreshExpireAt,
                    SessionId = result.Item4.Id.ToString(),
                    User = new registerResponse
                    {
                        UserId = result.Item3.Id,
                        UserName = result.Item3.UserName,
                        Email = result.Item3.Email,
                        FullName = result.Item3.FullName,
                        CreatedAt = result.Item3.CreatedAt
                    }
                };
                return Ok(response);
            }
            else
                return Unauthorized(result.Item2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }
}

