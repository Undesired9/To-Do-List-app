using Microsoft.EntityFrameworkCore;
using To_Do_list_app.Models;

namespace To_Do_list_app.Data;

public static class DbSeeder
{
    private static readonly (string Title, bool Completed, TaskCategory[] Categories)[] Samples =
    {
        ("Complete project proposal draft", true, new[] { TaskCategory.Work, TaskCategory.Health }),
        ("Go for a 30min run", true, new[] { TaskCategory.Health, TaskCategory.Fitness }),
        ("Buy groceries (milk, eggs, bread)", true, new[] { TaskCategory.Personal, TaskCategory.Shopping }),
        ("Walk the dog", false, new[] { TaskCategory.Personal }),
        ("Team meeting at 2 PM", false, new[] { TaskCategory.Work }),
        ("Pay electricity bill", false, new[] { TaskCategory.Personal }),
        ("Call Mom", false, new[] { TaskCategory.Personal })
    };

    /// <summary>Inserts the sample planner tasks when the database has none.</summary>
    public static async Task SeedAsync(PlannerDbContext db, DateOnly today)
    {
        if (await db.Tasks.AnyAsync())
        {
            return;
        }

        var stamp = DateTime.UtcNow;
        for (var i = 0; i < Samples.Length; i++)
        {
            var (title, completed, categories) = Samples[i];
            db.Tasks.Add(new TaskItem
            {
                Title = title,
                Completed = completed,
                DueDate = today,
                CreatedAt = stamp.AddMilliseconds(i),
                Categories = categories
                    .Select(c => new TaskItemCategory { Category = c })
                    .ToList()
            });
        }

        await db.SaveChangesAsync();
    }
}
