
namespace SimpleBankAPI.Application.Interfaces;

public interface IUserUseCase
{
    Task<(ErrorTypeUsecase?, string?, User?)> CreateUser(User user);
}
