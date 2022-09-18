namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.Mapper
{
    public class AccountMapper
    {
        internal static IEnumerable<Account>? Map(IEnumerable<dynamic> dataDb)
        {
            if (dataDb is null) return null;
            return dataDb.Select(x => new Account
            {
                Id = (int)x.id,
                UserId = (int)x.user_id,
                Balance = (decimal)x.balance,
                Currency = Enum.Parse<Currency>(x.currency),
                CreatedAt = (DateTime)x.created_at
            });
        }

        internal static Account? Map(dynamic x)
        {
            if (x is null) return null;
            return new Account
            {
                Id = (int)x.id,
                UserId = (int)x.user_id,
                Balance = (decimal)x.balance,
                Currency = Enum.Parse<Currency>(x.currency),
                CreatedAt = (DateTime)x.created_at,
            };
        }

    }
}
