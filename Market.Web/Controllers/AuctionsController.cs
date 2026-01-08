using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Market.Web.Data;
using Market.Web.Models;

namespace Market.Web.Controllers;

[Authorize] 
public class AuctionsController : Controller
{
    private readonly IAuctionRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuctionsController(IAuctionRepository repository, UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    // GET: Auctions (Lista wszystkich aukcji)
    [AllowAnonymous] // Listę mogą widzieć niezalogowani
    public async Task<IActionResult> Index()
    {
        var auctions = await _repository.GetAllAsync();
        return View(auctions);
    }
     [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var auction = await _repository.GetByIdAsync(id.Value);

        if (auction == null)
        {
            return NotFound();
        }

        return View(auction);
    }
    // GET: Auctions/Create (Formularz)
    public IActionResult Create()
    {
        return View();
    }

    // POST: Auctions/Create (Odbiór danych z formularza)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Auction auction)
    {
        
        var user = await GetCurrentUserAsync();
        if (user == null) return Challenge(); 

        
        auction.UserId = user.Id;
        auction.CreatedAt = DateTime.Now;
        auction.AuctionStatus = AuctionStatus.Active;

        
        ModelState.Remove("UserId");
        ModelState.Remove("User");

        
        if (ModelState.IsValid)
        {
            _context.Add(auction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        return View(auction);
    }
    private async Task<ApplicationUser> GetCurrentUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user;
    }
}