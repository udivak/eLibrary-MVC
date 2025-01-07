using eLibrary.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using eLibrary.Models; 
using eLibrary.Services;

using Microsoft.EntityFrameworkCore; // Required for EF Core

var builder = WebApplication.CreateBuilder(args);
// Register EmailService with dependency injection
builder.Services.AddTransient<EmailService>();
// Register DB_context with dependency injection and use SQLite
builder.Services.AddDbContext<DB_context>(options =>
    options.UseSqlite("Data Source=eLibraryDB.db"));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register IEmailService with EmailService
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddHostedService<WaitlistService>();

// Add IHttpContextAccessor and session services
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache();  // For session storage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static file and session middleware should come before routing
app.UseStaticFiles();
app.UseSession(); // Ensure this is placed before UseRouting

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Custom route (not part of default routing)
app.MapControllerRoute(
    name: "welcome",
    pattern: "welcome",
    defaults: new { controller = "Home", action = "Index" });

app.Run();