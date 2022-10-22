namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.Mapper
{
    public class DocumentMapper
    {
        internal static IEnumerable<Document>? Map(IEnumerable<dynamic> dataDb)
        {
            if (dataDb is null) return null;
            return dataDb.Select(x => new Document
            {
                Id = Guid.Parse(x.id),
                AccountId = (int)x.account_id,
                FileName = (string)x.file_name,
                URI = (string)x.uri,
                ContentType = (string)x.content_type,
                CreatedAt = (DateTime)x.created_at
            });
        }

        internal static Document? Map(dynamic x)
        {
            if (x is null) return null;
            return new Document
            {
                Id = Guid.Parse(x.id),
                AccountId = (int)x.account_id,
                FileName = (string)x.file_name,
                URI = (string)x.uri,
                ContentType = (string)x.content_type,
                CreatedAt = (DateTime)x.created_at
            };
        }

    }
}
