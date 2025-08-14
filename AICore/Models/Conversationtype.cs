namespace AICore.Models;

public class Conversationtype
{
    public int Pkconversationtypeid { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();
}