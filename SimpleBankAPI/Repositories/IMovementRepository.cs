using SimpleBankAPI.Models;

namespace SimpleBankAPI.Repositories;

public interface IMovementRepository
{
    Task<IEnumerable<MovementModel>> ReadByAccount(int accountId);
    Task<IEnumerable<MovementModel>> ReadById(int accountId, int id);
    Task<IEnumerable<MovementModel>> ReadAll();
    Task<(bool, int?)> Create(MovementModel dataModel);
    Task<bool> Update(MovementModel dataModel);
    Task<bool> Delete(int id);
}
