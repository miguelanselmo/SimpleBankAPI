using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Controllers;

[Route("simplebankapi/v1/[controller]")]
[ApiController]
public class TransferController : Controller
{
    private readonly ILogger<TransferController> _logger;
    private readonly ITransferRepository _repository;
    //private readonly ITransferUseCase _useCase;

    public TransferController(ILogger<TransferController> logger, ITransferRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }


    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost(Name = "CreateTransfer")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> Post([FromBody] transferRequest request)
    {
        try
        {
            if (Request.Headers.TryGetValue("Authorization", out StringValues authToken))
            {
                string authHeader = authToken.First();
                string token = authHeader.Substring("Bearer ".Length).Trim();
            }
            else
                return BadRequest("Missing Authorization Header.");
            var dataModel = new TransferModel
            {
                Amount = request.amount,
                FromAccountId = request.from_account_id,
                ToAccountId = request.to_account_id
            };
            return await _repository.Create(dataModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
   }
}

public struct transferRequest
{
    public decimal amount { get; set; }
    public int from_account_id { get; set; }
    public int to_account_id { get; set; }    
}