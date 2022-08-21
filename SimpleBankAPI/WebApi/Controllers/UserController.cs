using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Core.Usecases;
using SimpleBankAPI.WebApi.Models;
using Microsoft.Extensions.Primitives;
using System.Net;
using SimpleBankAPI.Infrastructure.Providers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SimpleBankAPI.WebApi.Controllers;

[Route("simplebankapi/v1/[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserUseCase _useCase;
    private readonly IAuthenticationProvider _provider;

    public UserController(ILogger<UserController> logger, IUserUseCase useCase, IAuthenticationProvider provider)
    {
        _logger = logger;
        _useCase = useCase;
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
    [HttpPost("login", Name = "Login")]
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
            var result = await _useCase.Login(account);
            if (result.Item1)
            {
                loginResponse response = new loginResponse
                {
                    AccessToken = result.Item4.TokenAccess,
                    AccessTokenExpiresAt = result.Item4.TokenAccessExpireAt,
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
    [HttpPost("logout", Name = "Logout")]
    [ProducesResponseType(typeof(Session), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<loginResponse>> Logout()
    {
        try
        {
            if (!Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                return BadRequest("Missing Authorization Header.");
            var resultClaims = _provider.GetClaimSession(authToken);
            if (!resultClaims.Item1)
                return BadRequest(resultClaims.Item2);
            /*
            var resultAuth = _provider.GetToken(authToken);
            if (!resultAuth.Item1)
                return BadRequest("Missing Session info in Token.");
            var resultClaims = _provider.GetClaimSession(resultAuth.Item2);
            if (!resultClaims.Item1)
                return BadRequest("Missing Session info in Token.");
            */
            var result = await _useCase.Logout(resultClaims.Item3);
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
}

/*
public struct registerRequest
{
    [JsonPropertyNameAttribute("user_name")]
    public string UserName { get; set; }
    [JsonPropertyNameAttribute("email")]
    public string Email { get; set; }
    [JsonPropertyNameAttribute("password")]
    public string Password { get; set; }
    [JsonPropertyNameAttribute("full_name")]
    public string FullName { get; set; }    
}

public struct registerResponse
{
    [JsonPropertyNameAttribute("user_id")]
    public int UserId { get; set; }
    [JsonPropertyNameAttribute("user_name")]
    public string UserName { get; set; }
    [JsonPropertyNameAttribute("email")]
    public string Email { get; set; }
    [JsonPropertyNameAttribute("full_name")]
    public string FullName { get; set; }
    [JsonPropertyNameAttribute("created_at")]
    public DateTime CreatedAt { get; set; }
}
*/
/*
public struct loginRequest
{
    public string user_name { get; set; }
    public string password { get; set; }    
}

public struct loginResponse
{
    public string access_token { get; set; }
    public DateTime access_token_expires_at { get; set; }
    public string session_id { get; set; }
    public registerResponse user { get; set; }
}
*/