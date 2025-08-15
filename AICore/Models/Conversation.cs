namespace AICore.Models;

public partial class Conversation
{
    public Guid Pkconversationid { get; set; }

    public Guid Fksecurityobjectowner { get; set; }

    public Guid? Fkparentid { get; set; }

    public string Title { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public string Description { get; set; } = null!;

    public string Serializedchat { get; set; } = null!;

    public virtual Securityobject FksecurityobjectownerNavigation { get; set; } = null!;
}
