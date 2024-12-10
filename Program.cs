using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using eLibrary.Models; // Make sure to include your models namespace
using Microsoft.EntityFrameworkCore; // Required for EF Core

var builder = WebApplication.CreateBuilder(args);

// Register DB_context with dependency injection and use SQLite
builder.Services.AddDbContext<DB_context>(options =>
    options.UseSqlite("Data Source=eLibraryDB.db")); // Point to your SQLite file

// Add services to the container.
builder.Services.AddControllersWithViews();
//added
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{

options.IdleTimeout = TimeSpan.FromMinutes(30);
});
//
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
//added
app.UseStaticFiles();
app.UseSession();
//
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Custom route (not part of default routing)
app.MapControllerRoute(
    name: "welcome",
    pattern: "welcome",
    defaults: new {controller = "Home", action = "Index"});