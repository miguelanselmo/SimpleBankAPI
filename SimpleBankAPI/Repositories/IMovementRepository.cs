using SimpleBankAPI.Models;

namespace SimpleBankAPI.Repositories;

public interface IMovementRepository
{
    Task<bool> Delete(int id);
    Task<MovementModel?> Read(int id);
    Task<IEnumerable<MovementModel>> ReadByAccount(int id);
    Task<IEnumerable<MovementModel>> Read();
    Task<bool> Create(MovementModel dataModel);
    Task<bool> Update(MovementModel dataModel);
}
