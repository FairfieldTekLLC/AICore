namespace AICore.Controllers.ViewModels;

public class ConversationVm
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ConversationText { get; set; }

    public Guid? ParentId { get; set; }
}