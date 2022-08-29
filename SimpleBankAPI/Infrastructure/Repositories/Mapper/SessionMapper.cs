using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Repositories.Mapper
{
    public class SessionMapper
    {
        internal static IEnumerable<Session>? Map(IEnumerable<dynamic> dataDb)
        {
            if (dataDb is null) return null;
            IEnumerable<Session> sessionList = dataDb.Select(x => new Session
            {
                Id = Guid.Parse(x.id),
                UserId = (int)x.user_id,
                Active = (bool)x.active,
                CreatedAt = (DateTime)x.created_at,
                TokenRefresh = (string)x.refresk_token,
                TokenRefreshExpireAt = (DateTime)x.refresk_token_expire_at,
            });
            return sessionList;
        }

        internal static Session? Map(dynamic x)
        {
            if (x is null) return null;
            return new Session
            {
                Id = Guid.Parse(x.id),
                UserId = (int)x.user_id,
                Active = (bool)x.active,
                CreatedAt = (DateTime)x.created_at,
                TokenRefresh = (string)x.refresk_token,
                TokenRefreshExpireAt = (DateTime)x.refresk_token_expire_at,
            };
        }
    }
}
