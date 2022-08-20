using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Repositories;

public interface IMovementRepository
{
    Task<IEnumerable<Movement>?> ReadByAccount(int accountId);
    Task<IEnumerable<Movement>?> ReadById(int accountId, int id);
    Task<IEnumerable<Movement>?> ReadAll();
    Task<(bool, int?)> Create(Movement data);
    Task<(bool, int?)> CreateLog(Transfer data);
    Task<bool> Update(Movement dataModel);
    Task<bool> Delete(int id);
}
