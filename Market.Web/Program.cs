using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Market.Web.Data;
using Market.Web.Models;
using Market.Web.Repositories;
using Market.Web.Services;

// FIX: To jest kluczowe dla Postgresa przy pracy z DateTime.Now.
// Bez tego aplikacja wyrzuci błąd przy zapisie daty rejestracji aukcji.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Pobieranie Connection Stringa (zmienna środowiskowa Coolify ma priorytet)
var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
var configConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var connectionString = !string.IsNullOrEmpty(envConnectionString) 
    ? envConnectionString 
    : configConnectionString 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Wykrywamy typ bazy na podstawie struktury Connection Stringa
bool isPostgres = connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) || 
                  connectionString.Contains("postgres", StringComparison.OrdinalIgnoreCase);

// 1. Konfiguracja DbContext
if (isPostgres)
{
    // Produkcja (Coolify) - PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
    
    Console.WriteLine("--> Używanie bazy PostgreSQL");
}
else
{
    // Lokalnie - SQLite
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
        
    Console.WriteLine("--> Używanie bazy SQLite");
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<Market.Web.Services.IAdminService, Market.Web.Services.AdminService>(); 
builder.Services.AddHttpClient<IADescriptionService, OpenRouterAiService>();
builder.Services.AddScoped<IAuctionProcessingService, AuctionProcessingService>(); 

var app = builder.Build();

// Konfiguracja potoku HTTP
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

app.UseAuthentication(); 
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
        bool dbIsPostgres = context.Database.ProviderName?.Contains("PostgreSQL") ?? false;

        if (dbIsPostgres)
        {
            logger.LogInformation("Wykryto Postgres. Rozpoczynam migrację struktur bazy danych...");
            
            await context.Database.MigrateAsync();
        }
        else
        {

        }
        await DbSeeder.SeedRolesAndAdminAsync(services);
        logger.LogInformation("Baza danych gotowa.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Wystąpił błąd podczas inicjalizacji/migracji bazy danych.");
    }
}

app.Run();