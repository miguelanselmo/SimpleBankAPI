using System;

namespace SimpleBankAPI.Models;
public class SessionModel
{
    public string TokenAccess { get; set; }
    public DateTime TokenAccessExpireAt { get; set; }
    public string TokenRefresh { get; set; }
    public DateTime TokenRefreshExpireAt { get; set; }
    public Guid SessionId { get; set; }
}
