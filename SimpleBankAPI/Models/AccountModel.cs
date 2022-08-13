using SimpleBankAPI.Models.Enums;


namespace SimpleBankAPI.Models;

public class AccountModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Balance { get; set; }
    public CurrencyEnum Currency { get; set; }
    public DateTime CreatedAt { get; set; }
}





