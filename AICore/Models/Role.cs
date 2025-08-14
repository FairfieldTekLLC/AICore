namespace AICore.Models;

public class Role
{
    public int Pkroleid { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();
}