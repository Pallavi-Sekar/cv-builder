using Microsoft.EntityFrameworkCore;
using CVBuilder.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Use SQLite as the database provider.
builder.Services.AddDbContext<CvBuilderContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ASP.NET Identity services and configure it.
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false) // Set to false for easier testing
    .AddEntityFrameworkStores<CvBuilderContext>();

// Add other services as needed (e.g., PDF generation, etc.)
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Ensure authentication middleware is in place.
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add the authentication middleware.
app.UseAuthentication(); // <-- This ensures that authentication works properly for login, register, etc.
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
