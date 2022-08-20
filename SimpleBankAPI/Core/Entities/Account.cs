using SimpleBankAPI.Core.Enums;


namespace SimpleBankAPI.Core.Entities;

public class Account
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Balance { get; set; }
    public Currency Currency { get; set; }
    public DateTime CreatedAt { get; set; }
}





