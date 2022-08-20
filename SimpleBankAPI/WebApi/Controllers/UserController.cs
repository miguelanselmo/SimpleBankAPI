using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories;
using SimpleBankAPI.Core.Usecases;
using System.Text.Json.Serialization;
using SimpleBankAPI.WebApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SimpleBankAPI.WebApi.Controllers;

[Route("simplebankapi/v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> logger;
    private readonly IUserUseCase useCase;

    public UserController(ILogger<UserController> logger, IUserUseCase useCase)
    {
        this.logger = logger;
        this.useCase = useCase;
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
            var result = await useCase.CreateUser(account);
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
            logger.LogError(ex.Message, ex.InnerException);
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
            var result = await useCase.Login(account);
            if (result.Item1)
            {
                //Response.Headers.Add("Authorization", (result.Item2);
                loginResponse response = new loginResponse
                {
                    AccessToken = result.Item4.TokenAccess,
                    AccessTokenExpiresAt = result.Item4.TokenAccessExpireAt,
                    SessionId = result.Item4.SessionId.ToString(),
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
            logger.LogError(ex.Message, ex.InnerException);
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