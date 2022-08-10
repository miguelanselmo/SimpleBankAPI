using SimpleBankAPI.Models;

namespace SimpleBankAPI.Repositories;

public interface IUserRepository
{
    Task<bool> DeleteUser(int id);
    Task<UserModel?> GetUser(int id);
    Task<IEnumerable<UserModel>> GetUsers();
    Task<bool> InsertUser(UserModel issue);
    Task<bool> UpdateUser(UserModel issue);
}
