using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;
using SimpleBankAPI.Usecases;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SimpleBankAPI.Controllers;

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
            var dataModel = new UserModel
            {
                UserName = request.user_name,
                Email = request.email,
                Password = request.password,
                FullName = request.full_name
            };
            var result = await useCase.CreateUser(dataModel);
            if (result.Item1)
            {
                return Ok(new registerResponse
                {
                    user_id = result.Item3.Id,
                    user_name = result.Item3.UserName,
                    email = result.Item3.Email,
                    full_name = result.Item3.FullName
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
            UserModel dataModel = new UserModel
            {

                UserName = request.user_name,
                Password = request.password
            };
            var result = await useCase.Login(dataModel);
            if (result.Item1)
            {
                //Response.Headers.Add("Authorization", (result.Item2);
                loginResponse response = new loginResponse
                {
                    access_token = result.Item4.TokenAccess,
                    access_token_expires_at = result.Item4.TokenAccessExpireAt,
                    session_id = result.Item4.SessionId.ToString(),
                    user = new registerResponse
                    {
                        user_id = result.Item3.Id,
                        user_name = result.Item3.UserName,
                        email = result.Item3.Email,
                        full_name = result.Item3.FullName,
                        created_at = result.Item3.CreatedAt
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


public struct errorResponse
{
    public string message { get; set; }
}

public struct registerRequest
{
    public string user_name { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public string full_name { get; set; }    
}

public struct registerResponse
{
    public int user_id { get; set; }
    public string user_name { get; set; }
    public string email { get; set; }
    public string full_name { get; set; }
    public DateTime created_at { get; set; }
}

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