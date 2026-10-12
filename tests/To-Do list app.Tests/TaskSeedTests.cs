using Microsoft.EntityFrameworkCore;
using To_Do_list_app.Data;
using To_Do_list_app.Models;

namespace To_Do_list_app.Tests;

public class TaskSeedTests
{
    [Fact]
    public async Task Seed_populates_seven_sample_tasks_when_empty()
    {
        using var db = TestDb.New();

        await DbSeeder.SeedAsync(db, new DateOnly(2026, 10, 26));

        Assert.Equal(7, await db.Tasks.CountAsync());
        Assert.Equal(3, await db.Tasks.CountAsync(t => t.Completed));
        Assert.All(
            await db.Tasks.ToListAsync(),
            t => Assert.Equal(new DateOnly(2026, 10, 26), t.DueDate));
    }

    [Fact]
    public async Task Seed_is_a_noop_when_tasks_already_exist()
    {
        using var db = TestDb.New();

        await DbSeeder.SeedAsync(db, new DateOnly(2026, 10, 26));
        await DbSeeder.SeedAsync(db, new DateOnly(2026, 10, 26));

        Assert.Equal(7, await db.Tasks.CountAsync());
    }
}
