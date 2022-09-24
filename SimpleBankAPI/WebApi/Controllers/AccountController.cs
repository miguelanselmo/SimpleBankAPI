using SimpleBankAPI.Application.Interfaces;
using SimpleBankAPI.Infrastructure.Ports.Providers;
using SimpleBankAPI.WebApi.Models;

namespace SimpleBankAPI.WebApi.Controllers;

[Route("simplebankapi/v1/[controller]")]
[ApiController]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountUseCase _useCase;
    private readonly ISessionUseCase _sessionUseCase;
    private readonly IAuthenticationProvider _provider;


    public AccountController(ILogger<AccountController> logger, IAccountUseCase useCase, ISessionUseCase sessionUseCase, IAuthenticationProvider provider)
    {
        _logger = logger;
        _useCase = useCase;
        _sessionUseCase = sessionUseCase;
        _provider = provider;
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
            var resultClaims = _provider.GetClaims(Request.Headers.Authorization);
            if (!resultClaims.Item1)
                return BadRequest(resultClaims.Item2);
            var resultSession = await _sessionUseCase.CheckSession(resultClaims.Item3);            
            if (resultSession.Item1 is not null)
                return Unauthorized(resultSession.Item2);

            var account = new Account
            {
                Balance = request.Amount,
                Currency = (Currency)Enum.Parse(typeof(Currency), request.Currency),
                UserId = resultClaims.Item3.UserId
            };
            var result = await _useCase.CreateAccount(account);
            
            switch(result.Item1)
            {
                case null:
                    return Created(String.Empty, new createAccountResponse
                    {
                        AccountId = result.Item3.Id,
                        Balance = result.Item3.Balance,
                        Currency = result.Item3.Currency.ToString(),
                    });
                case ErrorTypeUsecase.Business:
                    return BadRequest(result.Item2);
                case ErrorTypeUsecase.System:
                default:
                    return Problem(result.Item2);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
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
            var resultClaims = _provider.GetClaims(Request.Headers.Authorization);
            if (!resultClaims.Item1)
                return BadRequest(resultClaims.Item2);
            var resultSession = await _sessionUseCase.CheckSession(resultClaims.Item3);
            if (resultSession.Item1 is not null)
                return Unauthorized(resultSession.Item2);

            var result = await _useCase.GetAccounts(resultClaims.Item3.UserId);

            switch (result.Item1)
            {
                case null:
                    return Ok(result.Item3.Select(x => new account
                    {
                        AccountId = x.Id,
                        Balance = x.Balance,
                        Currency = x.Currency.ToString(),
                        CreatedAt = x.CreatedAt
                    }));
                case ErrorTypeUsecase.Business:
                    return BadRequest(result.Item2);
                case ErrorTypeUsecase.System:
                default:
                    return Problem(result.Item2);
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
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
            var resultClaims = _provider.GetClaims(Request.Headers.Authorization);
            if (!resultClaims.Item1)
                return BadRequest(resultClaims.Item2);
            var resultSession = await _sessionUseCase.CheckSession(resultClaims.Item3);
            if (resultSession.Item1 is not null)
                return Unauthorized(resultSession.Item2);

            var result = await _useCase.GetAccountMovements(resultClaims.Item3.UserId, id);

            switch (result.Item1)
            {
                case null:
                    if (result.Item3 is null)
                        return NotFound(result.Item2);
                    else
                        return Ok(new account
                        {
                            AccountId = result.Item3.Id,
                            Balance = result.Item3.Balance,
                            Currency = result.Item3.Currency.ToString(),
                            CreatedAt = result.Item3.CreatedAt,
                            Movs = result.Item4.Select(x => new movements
                            {
                                Amount = x.Amount,
                                CreatedAt = (DateTime)x.CreatedAt
                            })
                        });
                case ErrorTypeUsecase.Business:
                    return NotFound(result.Item2);
                case ErrorTypeUsecase.System:
                default:
                    return Problem(result.Item2);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.InnerException);
            return Problem(ex.Message);
        }
    }
}
