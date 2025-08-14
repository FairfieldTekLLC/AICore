namespace AICore.Models;

public class Conversation
{
    public Guid Pkconversationid { get; set; }

    public Guid Fksecurityobjectowner { get; set; }

    public Guid? Fkparentid { get; set; }

    public string Title { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public string Description { get; set; } = null!;

    public string? SerializedChat { get; set; }

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();

    public virtual Securityobject FksecurityobjectownerNavigation { get; set; } = null!;
}