using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Market.Web.Data;
using Market.Web.Models;

namespace Market.Web.Controllers;

[Authorize] // Tylko zalogowani widzą aukcje
public class AuctionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuctionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Auctions (Lista wszystkich aukcji)
    [AllowAnonymous] // Listę mogą widzieć niezalogowani
    public async Task<IActionResult> Index()
    {
        // Pobieramy aukcje i dołączamy dane autora (Include)
        var auctions = await _context.Auctions
            .Include(a => a.User) 
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
            
        return View(auctions);
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
        
        var user = await _userManager.GetUserAsync(User);
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
}