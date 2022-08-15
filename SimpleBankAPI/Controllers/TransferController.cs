using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Controllers;

[Route("simplebankapi/v1/[controller]")]
[ApiController]
public class TransferController : Controller
{
    private readonly ILogger<TransferController> _logger;
    private readonly ITransferRepository _repository;

    public TransferController(ILogger<TransferController> logger, ITransferRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet(Name = "GetTransfers")]
    [ProducesResponseType(typeof(IEnumerable<TransferModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransferModel>> Get()
    {
        try
        {
            var transfers = await _repository.ReadAll();
            return Ok(transfers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("{id}", Name = "GetTransfer")]
    [ProducesResponseType(typeof(TransferModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransferModel>> GetById(int id)
    {
        try
        {
            var transfer = await _repository.ReadById(id);
            if (transfer is null)
            {
                return NotFound();
            }
            return Ok(transfer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost(Name = "CreateTransfer")]
    [ProducesResponseType(typeof(IEnumerable<bool>), StatusCodes.Status201Created)]
    public async Task<ActionResult<bool>> Post([FromBody] TransferModel transfer)
    {
        return await _repository.Create(transfer);
    }

}