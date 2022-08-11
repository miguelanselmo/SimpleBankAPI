namespace SimpleBankAPI.Models.Dtos;

public class TransferDto
{
    public int Id { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
