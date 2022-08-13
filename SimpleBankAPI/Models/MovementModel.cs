namespace SimpleBankAPI.Models;

public class MovementModel
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
