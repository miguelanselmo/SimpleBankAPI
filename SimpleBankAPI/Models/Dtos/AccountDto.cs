using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SimpleBankAPI.Models.Dtos;

public class AccountDto
{
    public int Id { get; set; }
    public string Owner { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; }
    public DateTime CreatedAt { get; set; }
}





