namespace SimpleBankAPI.Core.Entities;

public class Document
{
    public Guid Id { get; set; }
    public int AccountId { get; set; }
    public string FileName { get; set; }
    public string URI { get; set; }
    public string ContentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public IFormFile Content { get; set; }

    public static implicit operator Document((bool, string?, Document?) v)
    {
        throw new NotImplementedException();
    }
}





