namespace Market.Web.Repositories;

public interface IUnitOfWork
{
    IAuctionRepository Auctions { get; }
    IOrderRepository Orders { get; }
    IProfileRepository Profiles { get; }

    Task CompleteAsync();
}