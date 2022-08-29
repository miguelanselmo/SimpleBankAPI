using System.Text.Json.Serialization;

namespace SimpleBankAPI.WebApi.Models;

public struct registerRequest
{
    [JsonPropertyNameAttribute("user_name")]
    public string UserName { get; set; }
    [JsonPropertyNameAttribute("email")]
    public string Email { get; set; }
    [JsonPropertyNameAttribute("password")]
    public string Password { get; set; }
    [JsonPropertyNameAttribute("full_name")]
    public string FullName { get; set; }
}

public struct registerResponse
{
    [JsonPropertyNameAttribute("user_id")]
    public int UserId { get; set; }
    [JsonPropertyNameAttribute("user_name")]
    public string UserName { get; set; }
    [JsonPropertyNameAttribute("email")]
    public string Email { get; set; }
    [JsonPropertyNameAttribute("full_name")]
    public string FullName { get; set; }
    [JsonPropertyNameAttribute("created_at")]
    public DateTime CreatedAt { get; set; }
}


public struct loginRequest
{
    [JsonPropertyNameAttribute("user_name")]
    public string UserName { get; set; }
    [JsonPropertyNameAttribute("password")]
    public string Password { get; set; }
}

public struct loginResponse
{
    [JsonPropertyNameAttribute("access_token")]
    public string AccessToken { get; set; }
    [JsonPropertyNameAttribute("access_token_expires_at")]
    public DateTime AccessTokenExpiresAt { get; set; }
    [JsonPropertyNameAttribute("refresh_token")]
    public string RefreshToken { get; set; }
    [JsonPropertyNameAttribute("refresh_token_expires_at")]
    public DateTime RefreshTokenExpiresAt { get; set; }
    [JsonPropertyNameAttribute("session_id")]
    public string SessionId { get; set; }
    [JsonPropertyNameAttribute("user")]
    public registerResponse User { get; set; }
}

public struct renewloginRequest
{
    [JsonPropertyNameAttribute("refresh_token")]
    public string RefreshToken { get; set; }

}