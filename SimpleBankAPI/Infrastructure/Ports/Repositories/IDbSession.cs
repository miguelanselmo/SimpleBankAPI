namespace SimpleBankAPI.Infrastructure.Ports.Repositories;

public interface IDbSession : IDisposable
{
    public void Dispose();
}