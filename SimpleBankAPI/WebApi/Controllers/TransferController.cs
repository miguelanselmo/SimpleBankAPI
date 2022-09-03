using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Providers;
using SimpleBankAPI.WebApi.Models;
using SimpleBankAPI.Application.Interfaces;

namespace SimpleBankAPI.WebApi.Controllers;

[Route("simplebankapi/v1/[controller]")]
[ApiController]
public class TransferController : Controller
{
    private readonly ILogger<TransferController> _logger;
    private readonly ITransferUseCase _useCase;
    private readonly ISessionUseCase _sessionUseCase;
    private readonly IAuthenticationProvider _provider;

    public TransferController(ILogger<TransferController> logger, ITransferUseCase useCase, ISessionUseCase sessionUseCase, IAuthenticationProvider provider)
    {
        _logger = logger;
        _useCase = useCase;
        _sessionUseCase = sessionUseCase;
        _provider = provider;
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
            var resultClaims = _provider.GetClaims(Request.Headers.Authorization);
            if (!resultClaims.Item1)
                return BadRequest(resultClaims.Item2);
            var resultSession = await _sessionUseCase.CheckSession(resultClaims.Item3);
            if (!resultSession.Item1)
                return Unauthorized(resultSession.Item2);

            var account = new Transfer
            {
                Amount = request.Amount,
                FromAccountId = request.FromAccountId,
                ToAccountId = request.ToAccountId,
                UserId = resultClaims.Item3.UserId
            };
            var result = await _useCase.Transfer(account);
            return result.Item1 ? Ok(new transferResponse { Amount = request.Amount * (-1), Balance = result.Item3.Balance }) : BadRequest(result.Item2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }
}
