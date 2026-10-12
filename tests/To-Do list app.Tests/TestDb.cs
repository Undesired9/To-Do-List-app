using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using To_Do_list_app.Data;

namespace To_Do_list_app.Tests;

/// <summary>
/// Builds isolated, file-backed SQLite databases for tests so a real relational
/// engine (not the in-memory provider) exercises the EF Core model.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly string _path;
    private readonly DbContextOptions<PlannerDbContext> _options;

    public IDbContextFactory<PlannerDbContext> Factory { get; }

    private TestDb(string path, DbContextOptions<PlannerDbContext> options)
    {
        _path = path;
        _options = options;
        Factory = new PooledDbContextFactory<PlannerDbContext>(options);
    }

    /// <summary>Creates the schema and returns a harness whose <see cref="Factory"/> shares the database.</summary>
    public static TestDb Create()
    {
        var path = Path.Combine(Path.GetTempPath(), $"planner-test-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        using (var db = new PlannerDbContext(options))
        {
            db.Database.EnsureCreated();
        }

        return new TestDb(path, options);
    }

    /// <summary>Opens a fresh context against the harness database.</summary>
    public PlannerDbContext CreateContext() => new(_options);

    /// <summary>Convenience for tests that need a single context: creates the schema and returns one.</summary>
    public static PlannerDbContext New()
    {
        var path = Path.Combine(Path.GetTempPath(), $"planner-test-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        var db = new PlannerDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { File.Delete(_path); } catch { /* best-effort temp cleanup */ }
    }
}
