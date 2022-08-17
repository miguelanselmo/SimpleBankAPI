using SimpleBankAPI.Models;

namespace SimpleBankAPI.Repositories;

public interface IUserRepository
{    
    Task<UserModel?> ReadById(int id);
    Task<IEnumerable<UserModel>> ReadAll();
    Task<(bool,int?)> Create(UserModel dataModel);
    Task<bool> Update(UserModel dataModel);
    Task<bool> Delete(int id);
}
