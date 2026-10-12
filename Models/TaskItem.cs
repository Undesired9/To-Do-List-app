using To_Do_list_app.Data;

namespace To_Do_list_app.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    /// <summary>Optional note. Null when the user has not added one.</summary>
    public string? Description { get; set; }

    public bool Completed { get; set; }

    public DateOnly DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TaskItemCategory> Categories { get; set; } = new();
}
