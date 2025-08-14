namespace AICore.Controllers.ViewModels;

public class ScheduleVm
{
    public int Id { get; set; }
    public Guid ConversationId { get; set; }
    public string? ConversationName { get; set; }
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public bool Saturday { get; set; }
    public bool Sunday { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public bool Enabled { get; set; }

    public DateTime? LastExecuted { get; set; }
}