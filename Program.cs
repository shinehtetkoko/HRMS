using HRMS.Data;
using HRMS.Interfaces;
using HRMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Dependency Injection Registered
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

//DB Connection Configurations 
builder.Configuration.AddIniFile("dbConnection.conf", optional: false, reloadOnChange: true);

var host = builder.Configuration["DbSettings:Host"];
var port = builder.Configuration["DbSettings:Port"];
var database = builder.Configuration["DbSettings:TargetDb"];
var username = builder.Configuration["DbSettings:Username"];
var password = builder.Configuration["DbSettings:Password"];

string connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// GLOBAL AUTHORIZATION FILTER CONFIGURATION
// ==========================================
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// AUTHENTICATION (COOKIE) SERVICES
// ==========================================
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

var app = builder.Build();

// 3. MIDDLEWARES CONFIGURATION 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();