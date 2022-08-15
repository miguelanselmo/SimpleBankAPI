using SimpleBankAPI.Models;

namespace SimpleBankAPI.Repositories;

public interface IUserRepository
{
    Task<bool> Delete(int id);
    Task<UserModel?> ReadById(int id);
    Task<UserModel?> ReadByUsername(string userName);
    Task<IEnumerable<UserModel>> ReadAll();
    Task<(bool,int?)> Create(UserModel dataModel);
    Task<bool> Update(UserModel dataModel);
}
