namespace SimpleBankAPI.Infrastructure.Ports.Repositories;

public interface IMovementRepository
{
    Task<IEnumerable<Movement>?> ReadByAccount(int accountId);
    Task<Movement?> ReadById(int accountId, int id);
    //Task<IEnumerable<Movement>?> ReadAll();
    Task<(bool, int?)> Create(Movement data);
    Task<(bool, int?)> CreateLog(Transfer data);
    //Task<bool> Update(Movement dataModel);
    //Task<bool> Delete(int id);
}
