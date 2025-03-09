using Microsoft.EntityFrameworkCore;
using theParodyJournal.Models;
using theParodyJournal.Services.ML;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using theParodyJournal.Controllers;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TextSummarizerService>();

builder.Services.AddDbContext<UserContext>(options =>
{
    options.UseSqlServer("Server=NAYAN-COMP-01\\SQLEXPRESS;Database=USERDB;Trusted_Connection=True;TrustServerCertificate=True;");
}); 




builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<NewsRecommendationService>();
builder.Services.AddSession();
var app = builder.Build();
app.Lifetime.ApplicationStopping.Register(() =>
{
    string modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel.zip");
    if (File.Exists(modelPath))
    {
        try
        {
            File.Delete(modelPath);
            Console.WriteLine("MLModel.zip deleted on shutdown.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting MLModel.zip: {ex.Message}");
        }
    }
});
app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
