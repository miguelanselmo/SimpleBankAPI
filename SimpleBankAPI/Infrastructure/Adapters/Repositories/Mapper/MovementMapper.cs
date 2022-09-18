namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.Mapper
{
    public class MovementMapper
    {
        internal static IEnumerable<Movement>? Map(IEnumerable<dynamic> dataDb)
        {
            IEnumerable<Movement> MovementList = dataDb.Select(x => new Movement
            {
                Id = (int)x.id,
                AccountId = (int)x.account_id,
                Amount = (decimal)x.amount,
                Balance = (decimal)x.balance,
                CreatedAt = (DateTime)x.created_at,
            });
            return MovementList;
        }

        internal static Movement? Map(dynamic x)
        {
            return new Movement
            {
                Id = (int)x.id,
                AccountId = (int)x.account_id,
                Amount = (decimal)x.amount,
                Balance = (decimal)x.balance,
                CreatedAt = (DateTime)x.created_at,
            };
        }
    }
}
