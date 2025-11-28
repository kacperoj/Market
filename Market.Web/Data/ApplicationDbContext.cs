using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Market.Web.Models;

namespace Market.Web.Data;

// Pamiętaj o generycznym <ApplicationUser>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Tabele
    public DbSet<Auction> Auctions { get; set; } // Zmienilem na liczbe mnoga (Standard)
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<CompanyProfile> CompanyProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.UserProfile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade); 


        builder.Entity<UserProfile>()
            .HasOne(p => p.CompanyProfile)
            .WithOne(c => c.UserProfile)
            .HasForeignKey<CompanyProfile>(c => c.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade); 

        
        builder.Entity<UserProfile>()
            .OwnsOne(x => x.ShippingAddress);

        builder.Entity<CompanyProfile>()
            .OwnsOne(x => x.InvoiceAddress);
    }
}