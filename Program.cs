using PlayPao.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.UseMiddleware<AuthenticationMiddleware>();


// app.Use(async (context, next) =>
// {
//     var path = context.Request.Path;


//     if (path.HasValue && path.Value.StartsWith("/Auth"))
//     {
//         Console.WriteLine("Auth ");
//     }

//     // if (!string.IsNullOrWhiteSpace(cultureQuery))
//     // {
//     //     var culture = new CultureInfo(cultureQuery);

//     //     CultureInfo.CurrentCulture = culture;
//     //     CultureInfo.CurrentUICulture = culture;
//     // }

//     // Call the next delegate/middleware in the pipeline.
//     await next(context);
// });

if (app.Environment.IsDevelopment())
{
    app.Run();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.Run();
}