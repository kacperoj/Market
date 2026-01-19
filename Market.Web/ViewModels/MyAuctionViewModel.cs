using Market.Web.Models;

namespace Market.Web.ViewModels;

public class MyAuctionViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    
    public string ImageUrl { get; set; } = string.Empty;
    public string CreatedAtFormatted { get; set; } = string.Empty;
    public string EndDateFormatted { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    public bool IsSoldOut => Quantity <= 0;
    public bool IsExpired { get; set; }
    public bool IsCompanySale { get; set; }
    public bool GeneratedByAi { get; set; }

    public string StatusBadgeClass { get; set; } = string.Empty; // Np. "bg-success"
    public string StatusLabel { get; set; } = string.Empty;      // Np. "AKTYWNA"
    
    public bool IsBannedOrSuspended { get; set; }
    public string? BannedNote { get; set; }
}