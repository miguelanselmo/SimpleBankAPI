namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.Mapper
{
    public class UserMapper
    {
        internal static IEnumerable<User>? Map(IEnumerable<dynamic> dataDb)
        {
            if (dataDb is null) return null;
            IEnumerable<User> userList = dataDb.Select(x => new User
            {
                Id = (int)x.id,
                UserName = (string)x.username,
                Email = (string)x.email,
                Password = (string)x.password,
                FullName = (string)x.full_name,
                CreatedAt = (DateTime)x.created_at,
            });
            return userList;
        }

        internal static User? Map(dynamic x)
        {
            if (x is null) return null;
            return new User
            {
                Id = (int)x.id,
                UserName = (string)x.username,
                Email = (string)x.email,
                Password = (string)x.password,
                FullName = (string)x.full_name,
                CreatedAt = (DateTime)x.created_at,
            };
        }
    }
}
