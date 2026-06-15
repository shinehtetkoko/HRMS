using HRMS.Configurations;
using HRMS.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddIniFile("dbConnection.conf", optional: false, reloadOnChange: true);
var dbSettings = builder.Configuration.GetSection("DbSettings").Get<DbSettings>();
if (dbSettings != null)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(dbSettings.ConnectionString));
}

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
