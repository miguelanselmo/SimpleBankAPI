using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Repositories;

public interface ISessionRepository
{
    Task<IEnumerable<Session>?> ReadAll();
    Task<Session?> ReadById(Guid id);
    Task<IEnumerable<Session>?> ReadByUser(int userId);
    Task<bool> Create(Session data);
    Task<bool> Update(Session data);
    Task<bool> Delete(Guid id);
}
