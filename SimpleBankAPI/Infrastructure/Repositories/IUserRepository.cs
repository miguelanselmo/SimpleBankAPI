using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Repositories;

public interface IUserRepository
{    
    Task<UserModel?> ReadById(int id);
    Task<UserModel?> ReadByName(string name);
    Task<IEnumerable<UserModel>> ReadAll();
    Task<(bool,int?)> Create(UserModel dataModel);
    Task<bool> Update(UserModel dataModel);
    Task<bool> Delete(int id);
}
