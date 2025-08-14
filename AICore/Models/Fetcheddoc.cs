namespace AICore.Models;

public class Fetcheddoc
{
    public Guid Pkfetchdocid { get; set; }

    public string Memorykey { get; set; } = null!;

    public string Uri { get; set; } = null!;

    public string Body { get; set; } = null!;

    public Guid Fkentryid { get; set; }

    public string? Summary { get; set; }

    public virtual Entry Fkentry { get; set; } = null!;
}