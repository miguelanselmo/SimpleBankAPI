using System.Text.Json.Serialization;

namespace SimpleBankAPI.WebApi.Models;

public struct createAccountRequest
{
    [JsonPropertyNameAttribute("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyNameAttribute("currency")]
    public string Currency { get; set; }

}

public struct createAccountResponse
{
    [JsonPropertyNameAttribute("account_id")]
    public int AccountId { get; set; }
    [JsonPropertyNameAttribute("balance")]
    public decimal Balance { get; set; }
    [JsonPropertyNameAttribute("currency")]
    public string Currency { get; set; }
}

public struct account
{
    [JsonPropertyNameAttribute("account_id")]
    public int AccountId { get; set; }
    [JsonPropertyNameAttribute("balance")]
    public decimal Balance { get; set; }
    [JsonPropertyNameAttribute("currency")]
    public string Currency { get; set; }
    [JsonPropertyNameAttribute("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyNameAttribute("movs")]
    public IEnumerable<movements>? Movs { get; set; }
}

public struct movements
{
    [JsonPropertyNameAttribute("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyNameAttribute("created_at")]
    public DateTime CreatedAt { get; set; }
}
/*
public struct uploadDocumentRequest
{
    //[JsonPropertyNameAttribute("account_id")]
    //public int AccountId { get; set; }
    //[JsonPropertyNameAttribute("description")]
    //public string? Description { get; set; }
    //[JsonPropertyNameAttribute("file")]
    public IFormFile Content { get; set; }

}
*/
public struct document
{
    [JsonPropertyNameAttribute("id")]
    public string Id { get; set; }
    [JsonPropertyNameAttribute("account_id")]
    public int AccountId { get; set; }
    [JsonPropertyNameAttribute("file_name")]
    public string FileName { get; set; }
    [JsonPropertyNameAttribute("created_at")]
    public DateTime CreatedAt { get; set; }
}

/*
public struct document
{
    [JsonPropertyNameAttribute("id")]
    public string Id { get; set; }
    [JsonPropertyNameAttribute("account_id")]
    public int AccountId { get; set; }
    [JsonPropertyNameAttribute("file_name")]
    public string FileName { get; set; }
    [JsonPropertyNameAttribute("created_at")]
    public DateTime CreatedAt { get; set; }
}
*/