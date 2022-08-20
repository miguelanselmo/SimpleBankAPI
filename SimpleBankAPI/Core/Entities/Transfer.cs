namespace SimpleBankAPI.Core.Entities;

public class Transfer
{
    public int Id { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
}
