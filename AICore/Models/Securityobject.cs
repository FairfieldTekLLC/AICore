namespace AICore.Models;

public class Securityobject
{
    public Guid Activedirectoryid { get; set; }

    public string Fullname { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Pass { get; set; } = null!;

    public string Emailaddress { get; set; } = null!;

    public short Isgroup { get; set; }

    public short Isactive { get; set; }

    public string? Forename { get; set; }

    public string? Surname { get; set; }

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}