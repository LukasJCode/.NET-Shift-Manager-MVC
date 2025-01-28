using ShiftManager.Repos;
using ShiftManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Logging.AddConsole();

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepositorySQL>();
builder.Services.AddScoped<IJobRepository, JobRepositorySQL>();
builder.Services.AddScoped<IShiftRepository, ShiftRepositorySQL>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
