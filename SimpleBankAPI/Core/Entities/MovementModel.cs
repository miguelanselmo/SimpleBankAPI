namespace SimpleBankAPI.Core.Entities;

public class MovementModel
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
}
