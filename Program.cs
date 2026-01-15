using BlogApplication.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



// Add services to the container.
builder.Services.AddControllersWithViews();

// ?? ADD THIS FOR AUTHENTICATION ??
builder.Services.AddAuthentication("BlogAppAuth")
    .AddCookie("BlogAppAuth", options =>
    {
        options.LoginPath = "/Account/Login";           // Where to redirect if not logged in
        options.AccessDeniedPath = "/Account/AccessDenied"; // If user lacks permission
        options.Cookie.HttpOnly = true;                 // More secure
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Works in dev & prod
    });


var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ?? ADD THESE TWO LINES HERE ??
app.UseAuthentication();  // Reads the auth cookie
app.UseAuthorization();   // Checks roles/policies

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();