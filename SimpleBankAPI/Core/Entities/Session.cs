namespace SimpleBankAPI.Core.Entities;
public class Session
{
    public string TokenAccess { get; set; }
    public DateTime TokenAccessExpireAt { get; set; }
    public string TokenRefresh { get; set; }
    public DateTime TokenRefreshExpireAt { get; set; }
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Active { get; set; }
}
