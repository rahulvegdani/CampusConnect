using CampusConnect.Data;
using CampusConnect.Helpers;
using CampusConnect.Hubs;
using CampusConnect.Models;
using CampusConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------- DATABASE CONNECTION ----------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ---------------- IDENTITY WITH ROLES ----------------
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ✅ FIX: Redirect to login if not authenticated
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
});

// ---------------- SIGNALR (CHAT) ----------------
builder.Services.AddSignalR();

// ✅ USER ID PROVIDER (IMPORTANT FOR CHAT)
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

// ---------------- MVC ----------------
builder.Services.AddControllersWithViews();

// --------- Session ----------
builder.Services.AddSession();

builder.Services.AddScoped<EmailService>();

var app = builder.Build();

// ---------------- CREATE ROLES & DEFAULT ADMIN ----------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Student" };

    foreach (var role in roles)
    {
        var roleExist = await roleManager.RoleExistsAsync(role);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    string adminEmail = "admin@campus.com";
    string adminPassword = "Admin@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            CreatedAt = DateTime.Now
        };

        var createAdmin = await userManager.CreateAsync(newAdmin, adminPassword);

        if (createAdmin.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }

}

// ---------------- HTTP PIPELINE ----------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication(); // Identity Login
app.UseAuthorization();  // Role Authorization

// ---------------- ROUTING ----------------
app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ---------------- SIGNALR HUB ROUTE ----------------
app.MapHub<ChatHub>("/chatHub");

app.Run();
