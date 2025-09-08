using Npgsql;
using DotNetEnv;
using PlayPao.Config;
using PlayPao.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.UseMiddleware<AuthenticationMiddleware>();

Env.Load();

app.UseExceptionHandler("/Home/Error");
app.UseHsts();

var connectionString = DatabaseConfig.GetConnectionString(builder.Configuration, app.Environment);
await using var conn = new NpgsqlConnection(connectionString);

app.Run();
