using Market.Web.Core.DTOs;


namespace Market.Web.Services;

public interface IADescriptionService
{
    Task<AuctionDraftDto> GenerateFromImagesAsync(List<IFormFile> images);
}