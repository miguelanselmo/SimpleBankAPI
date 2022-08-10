using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SimpleBankAPI.Models;

public class AccountModel
{
    public int Id { get; set; }
    public string Owner { get; set; }
    public decimal Balance { get; set; }
    public CurrencyEnum Currency { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum CurrencyEnum
{
    [Description("USD")] USD,
    [Description("EUR")] EUR
}

public class AccountRequest
{
    //ID int64 `uri:"id" binding:"required,min=1"`
    [JsonPropertyName("amount") ]
    public decimal Amount { get; set; }
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
}



