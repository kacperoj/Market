using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Market.Web.Persistence;
using Market.Web.Core.Models;
using Market.Web.Repositories;
using Market.Web.Services;
using Market.Web.Authorization;

namespace Market.Web.Controllers;

[Authorize] 
public class AuctionsController : Controller
{
    private readonly IUnitOfWork _unitOfWork; 
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IADescriptionService _aiDescriptionService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuctionProcessingService _auctionProcessingService;

    public AuctionsController(
        IUnitOfWork unitOfWork, 
        IWebHostEnvironment webHostEnvironment, 
        IADescriptionService aiDescriptionService,
        UserManager<ApplicationUser> userManager, 
        IAuctionProcessingService auctionProcessingService)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
        _aiDescriptionService = aiDescriptionService;
        _userManager = userManager;
        _auctionProcessingService = auctionProcessingService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchString, string? category, decimal? minPrice, decimal? maxPrice, string? sortOrder)
    {
        var auctions = await _unitOfWork.Auctions.GetAllAsync();

        IEnumerable<Auction> query = auctions
            .Where(a => a.AuctionStatus == AuctionStatus.Active && a.EndDate > DateTime.Now);

        if (!string.IsNullOrEmpty(searchString))
            query = query.Where(s => s.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase) || s.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase));
        
        if (!string.IsNullOrEmpty(category)) query = query.Where(x => x.Category == category);
        if (minPrice.HasValue) query = query.Where(x => x.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(x => x.Price <= maxPrice.Value);

        query = sortOrder switch {
            "price_desc" => query.OrderByDescending(s => s.Price),
            "price_asc" => query.OrderBy(s => s.Price),
            _ => query.OrderByDescending(s => s.CreatedAt),
        };
        return View(query.ToList());
    }

    [Buyer]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var auction = await _unitOfWork.Auctions.GetByIdAsync(id.Value);
        return auction == null ? NotFound() : View(auction);
    }

    [Seller]
    public async Task<IActionResult> Create()
    {
        var user = await GetCurrentUserAsync();

        var userProfile = user != null ? await _unitOfWork.Profiles.GetByUserIdAsync(user.Id) : null;
        bool hasCompanyProfile = userProfile?.CompanyProfile != null;

        ViewBag.CanSellAsCompany = hasCompanyProfile;

        return View(new Auction { EndDate = DateTime.Now.AddDays(30) });
    }

    [Seller]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Auction auction, List<IFormFile> photos)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Challenge(); 

        auction.UserId = user.Id;
        auction.CreatedAt = DateTime.Now;
        auction.AuctionStatus = AuctionStatus.Active;

        if (auction.EndDate <= DateTime.Now)
             ModelState.AddModelError("EndDate", "Nieprawidłowa data.");

        ModelState.Remove("UserId"); ModelState.Remove("User"); ModelState.Remove("Images"); 

        if (ModelState.IsValid)
        {
            auction.Images = await _auctionProcessingService.ProcessUploadedImagesWebpAsync(photos);

            await _unitOfWork.Auctions.AddAsync(auction);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(auction);
    }
 
    [Seller]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var auction = await _unitOfWork.Auctions.GetByIdAsync(id.Value);
        if (auction == null) return NotFound();

        var user = await GetCurrentUserAsync();
        if (user == null || auction.UserId != user.Id) return Forbid();

        return View(auction);
    }

    [Seller]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Auction auction, List<IFormFile> photos)
    {
        if (id != auction.Id) return NotFound();
        var auctionToUpdate = await _unitOfWork.Auctions.GetByIdAsync(id);
        if (auctionToUpdate == null) return NotFound();

        var user = await GetCurrentUserAsync();
        if (user == null || auctionToUpdate.UserId != user.Id) return Forbid();

        ModelState.Remove("UserId"); ModelState.Remove("User"); ModelState.Remove("Images");

        if (ModelState.IsValid)
        {
            auctionToUpdate.Title = auction.Title;
            auctionToUpdate.Description = auction.Description;
            auctionToUpdate.Price = auction.Price;
            auctionToUpdate.Category = auction.Category;
            auctionToUpdate.Quantity = auction.Quantity;
            
            if (auction.EndDate > DateTime.Now) auctionToUpdate.EndDate = auction.EndDate;

            var newImages = await _auctionProcessingService.ProcessUploadedImagesWebpAsync(photos);
            if (auctionToUpdate.Images == null) auctionToUpdate.Images = new List<AuctionImage>();
            foreach(var img in newImages) auctionToUpdate.Images.Add(img);

            await _unitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(auctionToUpdate);
    }
    [Seller]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Challenge();

        var auctionToExpire = await _unitOfWork.Auctions.GetByIdAsync(id);
        
        if (auctionToExpire == null) return NotFound();

        if (auctionToExpire.UserId != user.Id) return Forbid();
        
        auctionToExpire.AuctionStatus = AuctionStatus.Cancelled;

        await _unitOfWork.CompleteAsync();

        TempData["SuccessMessage"] = "Oferta została zakończona.";
        return RedirectToAction(nameof(MyAuctions));
    }
    [Seller]
    [Authorize]
    public async Task<IActionResult> MyAuctions()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Challenge();

        var model = await _auctionProcessingService.GetUserAuctionsViewModelAsync(user.Id);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateDescription(List<IFormFile> photos)
    {
        if (photos == null || photos.Count == 0) return BadRequest(new { error = "Brak zdjęć." });
        try
        {
            return Json(await _aiDescriptionService.GenerateFromImagesAsync(photos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Błąd AI: " + ex.Message });
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync() => await _userManager.GetUserAsync(User);
}