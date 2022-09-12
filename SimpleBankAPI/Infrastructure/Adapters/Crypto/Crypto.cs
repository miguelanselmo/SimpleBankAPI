
namespace SimpleBankAPI.Infrastructure.Crypto;

public class Crypto
{
    public static string HashSecret(string secret)
    {
        return BCrypt.Net.BCrypt.HashPassword(secret);
    }

    public static bool VerifySecret(string secretHashed, string secretToVerify)
    {
        return BCrypt.Net.BCrypt.Verify(secretToVerify, secretHashed);
    }
}