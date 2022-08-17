﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SimpleBankAPI.Models;
using SimpleBankAPI.Models.Enums;
using SimpleBankAPI.Providers;
using SimpleBankAPI.Repositories;
using SimpleBankAPI.Usecases;

namespace SimpleBankAPI.Controllers;

[Route("simplebankapi/v1/[controller]")]
[ApiController]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;
    private readonly IAccountUseCase useCase;
    private readonly IAuthenticationProvider provider;
    

    public AccountController(ILogger<AccountController> logger, IAccountUseCase useCase, IAuthenticationProvider provider)
    {
        this.logger = logger;
        this.useCase = useCase;
        this.provider = provider;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost(Name = "CreateAccount")]
    [ProducesResponseType(typeof(IEnumerable<createAccountResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<createAccountResponse>> Post([FromBody] createAccountRequest request)
    {
        try
        {
            if (!Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                return BadRequest("Missing Authorization Header.");
            var resultAuth = provider.GetToken(authToken);
            if (!resultAuth.Item1)
                return BadRequest("Missing User info in Token.");
            var resultClaims = provider.GetClaimUser(resultAuth.Item2);
            if (!resultClaims.Item1)
                return BadRequest("Missing User info in Token.");

            var dataModel = new AccountModel
            {
                Balance = request.amount,
                Currency = Enum.Parse<CurrencyEnum>(request.currency),
                UserId = resultClaims.Item2.Id
            };
            var result = await useCase.CreateAccount(dataModel);
            if (result.Item1)
            {
                return Ok(new createAccountResponse
                {
                    account_id = result.Item3.Id,
                    balance = result.Item3.Balance,
                    currency = result.Item3.Currency.ToString(),
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

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet(Name = "GetAccounts")]
    [ProducesResponseType(typeof(IEnumerable<account>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<account>>> Get()
    {
        try
        {
            if (!Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                return BadRequest("Missing Authorization Header.");
            var resultAuth = provider.GetToken(authToken);
            if (!resultAuth.Item1)
                return BadRequest("Missing User info in Token.");
            var resultClaims = provider.GetClaimUser(resultAuth.Item2);
            if (!resultClaims.Item1)
                return BadRequest("Missing User info in Token.");

            var result = await useCase.GetAccounts(resultClaims.Item2.Id);
            return Ok(result.Item3.Select(x => new account
            {
                account_id = x.Id,
                balance = x.Balance,
                currency = x.Currency.ToString(),
                created_at = x.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("{id}", Name = "GetAccount")]
    [ProducesResponseType(typeof(account), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<account>> GetById(int id)
    {
        try
        {
            if (!Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                return BadRequest("Missing Authorization Header.");
            var resultAuth = provider.GetToken(authToken);
            if (!resultAuth.Item1)
                return BadRequest("Missing User info in Token.");
            var resultClaims = provider.GetClaimUser(resultAuth.Item2);
            if (!resultClaims.Item1)
                return BadRequest("Missing User info in Token.");

            var result = await useCase.GetAccountMovements(resultClaims.Item2.Id, id);
            if (result.Item3 is null)
                return NotFound(result.Item2);
            else
                return Ok(new account
                {
                    account_id = result.Item3.Id,
                    balance = result.Item3.Balance,
                    currency = result.Item3.Currency.ToString(),
                    created_at = result.Item3.CreatedAt,
                    movs = result.Item4.Select(x => new movements
                    {
                        amount = x.Amount,
                        created_at = x.CreatedAt
                    })
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex.InnerException);
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
    public IEnumerable<movements> movs { get; set; }
}

public struct movements
{
    public decimal amount { get; set; }
    public DateTime created_at { get; set; }
}
