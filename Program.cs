using Npgsql;
using DotNetEnv;
using PlayPao.Config;
using PlayPao.Middlewares;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
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
