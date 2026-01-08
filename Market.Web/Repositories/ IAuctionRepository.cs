using Market.Web.Models;

namespace Market.Web.Repositories;

public interface IAuctionRepository
{
    Task<List<Auction>> GetAllAsync();
    
    Task<Auction?> GetByIdAsync(int id);
    
    Task AddAsync(Auction auction);
    
    Task SaveChangesAsync();
}