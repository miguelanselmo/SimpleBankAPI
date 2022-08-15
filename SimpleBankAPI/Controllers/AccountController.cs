using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleBankAPI.Models;
using SimpleBankAPI.Models.Enums;
using SimpleBankAPI.Repositories;
using SimpleBankAPI.Usecases;

namespace SimpleBankAPI.Controllers;

[Route("simplebankapi/v1/[controller]")]
[ApiController]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountUseCase _useCase;

    public AccountController(ILogger<AccountController> logger, IAccountUseCase useCase)
    {
        _logger = logger;
        _useCase = useCase;
    }

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost(Name = "CreateAccount")]
    [ProducesResponseType(typeof(IEnumerable<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> Post([FromBody] createAccountRequest request)
    {
        try
        {
            var dataModel = new AccountModel
            {
                Balance = request.amount,
                Currency = Enum.Parse<CurrencyEnum>(request.currency)
            };
            var result = await _useCase.CreateAccount(dataModel);
            if (result.Item1)
            {
                return Ok(new createAccountResponse
                {
                    account_id = result.Item3.Id,
                    balance = result.Item3.Balance,
                    currency = result.Item3.Currency.ToString()
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
    [HttpGet(Name = "GetAccounts")]
    [ProducesResponseType(typeof(IEnumerable<account>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(registerResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccountModel>> Get(int userId)
    {
        try
        {
            var result = await _useCase.GetAccounts(userId);
            return Ok(result.Item3);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("{id}", Name = "GetAccount")]
    [ProducesResponseType(typeof(account), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<AccountModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<AccountModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(registerResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]

    public async Task<ActionResult<AccountModel>> GetById(int id)
    {
        try
        {
            var result = await _useCase.GetAccountDetails(id);
            if (result.Item3 is null)
                return NotFound(result.Item2);
            else
                return Ok(result.Item3);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }
}

public struct createAccountRequest {
    public decimal amount { get; set; }
    public string currency { get; set; }

}

public struct createAccountResponse
{
    public int account_id { get; set; }
    public decimal balance { get; set; }
    public string currency { get; set; }
}

public struct account
{
    public int account_id { get; set; }
    public decimal balance { get; set; }
    public string currency { get; set; }
    public DateTime created_at { get; set; }
}