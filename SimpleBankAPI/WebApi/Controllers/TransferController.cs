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

[Route("simplebankapi/v1/[controller]")]
[ApiController]
public class TransferController : Controller
{
    private readonly ILogger<TransferController> logger;
    //private readonly ITransferRepository repository;
    private readonly ITransferUseCase useCase;
    private readonly IAuthenticationProvider provider;
    
        public TransferController(ILogger<TransferController> logger, ITransferUseCase useCase, IAuthenticationProvider provider)
    {
        this.logger = logger;
        this.useCase = useCase;
        this.provider = provider;
    }


    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost(Name = "CreateTransfer")]
    [ProducesResponseType(typeof(transferResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> Post([FromBody] transferRequest request)
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
            
            var account = new Transfer
            {
                Amount = request.Amount,
                FromAccountId = request.FromAccountId,
                ToAccountId = request.ToAccountId,
                UserId = resultClaims.Item2.Id
            };
            var result = await useCase.Transfer(account);
            return result.Item1 ? Ok(new transferResponse { Amount = request.Amount * (-1), Balance = result.Item3.Balance }) : BadRequest(result.Item2);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
   }
}
/*
public struct transferRequest
{
    public decimal amount { get; set; }
    public int from_account_id { get; set; }
    public int to_account_id { get; set; }    
}

public struct transferResponse
{
    public decimal amount { get; set; }
    public decimal balance { get; set; }
}
*/