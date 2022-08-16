
using SimpleBankAPI.Models;

namespace SimpleBankAPI.Authentication;

public interface IAuthenticationHandler
{
    private static string? issuer;
    private static string? audience;
    private static string? key;
    public void Configure(IConfiguration config);

    public string GenerateToken();

    public string GenerateToken(UserModel user);
}
