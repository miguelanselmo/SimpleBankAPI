using System.Text.Json.Serialization;

namespace SimpleBankAPI.WebApi.Models;

public struct transferRequest
{
    [JsonPropertyNameAttribute("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyNameAttribute("from_account_id")]
    public int FromAccountId { get; set; }
    [JsonPropertyNameAttribute("to_account_id")]
    public int ToAccountId { get; set; }
}

public struct transferResponse
{
    [JsonPropertyNameAttribute("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyNameAttribute("balance")]
    public decimal Balance { get; set; }
}