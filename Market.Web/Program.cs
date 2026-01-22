using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Market.Web.Data;
using Market.Web.Models;
using Market.Web.Repositories;
using Market.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Konfiguracja bazy danych
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Rejestracja serwisów
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<Market.Web.Services.IAdminService, Market.Web.Services.AdminService>(); 
builder.Services.AddHttpClient<IADescriptionService, OpenRouterAiService>();
builder.Services.AddScoped<IAuctionProcessingService, AuctionProcessingService>(); 

var app = builder.Build();

// 2. Pipeline HTTP
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try 
    {
        if (app.Environment.IsDevelopment())
        {
            await DbSeeder.SeedRolesAndAdminAsync(services);
        }
        else
        {

            logger.LogInformation("Próba inicjalizacji bazy danych PostgreSQL...");
            context.Database.EnsureCreated();
            logger.LogInformation("Baza danych gotowa. Uruchamianie seedera...");
            await DbSeeder.SeedRolesAndAdminAsync(services);
            logger.LogInformation("Seedowanie zakończone.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Błąd krytyczny podczas inicjalizacji bazy danych.");
    }
}

app.Run();