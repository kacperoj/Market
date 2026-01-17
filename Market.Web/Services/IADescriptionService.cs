using Market.Web.DTOs;

namespace Market.Web.Services;

public interface IADescriptionService
{
    Task<AuctionDraftDto> GenerateFromImagesAsync(List<IFormFile> images);
}