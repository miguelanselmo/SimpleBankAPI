namespace SimpleBankAPI.Infrastructure.Ports.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T> ReadById(int id);
    Task<IEnumerable<T>> ReadAll();
    Task Create(T entity);
    void Delete(T entity);
    void Update(T entity);
}
