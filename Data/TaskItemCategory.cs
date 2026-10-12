using To_Do_list_app.Models;

namespace To_Do_list_app.Data;

/// <summary>Join row linking a <see cref="TaskItem"/> to a <see cref="TaskCategory"/>.</summary>
public class TaskItemCategory
{
    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public TaskCategory Category { get; set; }
}
