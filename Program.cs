using Microsoft.EntityFrameworkCore;
using To_Do_list_app.Components;
using To_Do_list_app.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDbContextFactory<PlannerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PlannerDb")));
builder.Services.AddScoped<To_Do_list_app.Services.TaskService>();
builder.Services.AddScoped<To_Do_list_app.Services.IPreferenceStore, To_Do_list_app.Services.ProtectedPreferenceStore>();
builder.Services.AddScoped<To_Do_list_app.Services.PreferenceService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PlannerDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, DateOnly.FromDateTime(DateTime.Today));
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
