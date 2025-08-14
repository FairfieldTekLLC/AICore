namespace AICore.Models;

public class Entry
{
    public Guid Pkentryid { get; set; }

    public Guid Fkconversationid { get; set; }

    public int Fkconversationtypeid { get; set; }

    public int Fkroleid { get; set; }

    public string? Text { get; set; }

    public int? Numberofresults { get; set; }

    public string? Resulttext { get; set; }

    public int Sequence { get; set; }

    public byte[]? Filedata { get; set; }

    public short Ishidden { get; set; }

    public string? Optionalfield1 { get; set; }

    public virtual ICollection<Fetcheddoc> Fetcheddocs { get; set; } = new List<Fetcheddoc>();

    public virtual Conversation Fkconversation { get; set; } = null!;

    public virtual Conversationtype Fkconversationtype { get; set; } = null!;

    public virtual Role Fkrole { get; set; } = null!;
}