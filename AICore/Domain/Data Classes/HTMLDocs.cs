namespace AICore.Domain.Data_Classes;

public class HtmlDocs
{
    public Guid Id { get; set; }
    public string? MemoryKey { get; set; }
    public string? Uri { get; set; }
    public string? Body { get; set; }
    public string? Summary { get; set; }
}