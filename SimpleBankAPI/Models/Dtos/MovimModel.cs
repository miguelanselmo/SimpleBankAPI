namespace SimpleBankAPI.Models.Dtos;

public class MovimDto
    
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
