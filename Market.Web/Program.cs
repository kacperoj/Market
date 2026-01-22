using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Market.Web.Data;
using Market.Web.Models;
using Market.Web.Repositories;
using Market.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
var configConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var connectionString = !string.IsNullOrEmpty(envConnectionString) 
    ? envConnectionString 
    : configConnectionString 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. Wybór bazy danych (Inteligentny)
// Zamiast pytać o Environment, sprawdzamy czy connection string wygląda na Postgresa (ma "Host=")
// Dzięki temu możesz mieć tryb Development na Coolify i używać Postgresa
if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) || 
    connectionString.Contains("postgres", StringComparison.OrdinalIgnoreCase))
{
    // PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // SQLite (lokalnie)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
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