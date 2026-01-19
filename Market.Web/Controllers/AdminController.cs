using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Market.Web.Services;
using Market.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace Market.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    private readonly UserManager<ApplicationUser> _userManager;
    public AdminController(IAdminService adminService, UserManager<ApplicationUser> userManager)
    {
        _adminService = adminService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string searchString, string sortOrder, int pageNumber = 1)
    {
        int pageSize = 10; 
        
        var model = await _adminService.GetUsersAsync(searchString, sortOrder, pageNumber, pageSize);
        
        return View(model);
    }

        [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlockUser(string id, string reason) 
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (user.IsBlocked)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);

            user.IsBlocked = false;  
            user.BlockReason = null; 
            
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Użytkownik został odblokowany.";
        }
        else
        {
            await _userManager.SetLockoutEnabledAsync(user, true); 
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            
            user.IsBlocked = true;   
            user.BlockReason = string.IsNullOrEmpty(reason) ? "Naruszenie regulaminu" : reason;
            
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Użytkownik został zablokowany.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Auctions(string searchString, string sortOrder, int pageNumber = 1)
    {
        int pageSize = 10;
        var model = await _adminService.GetAuctionsAsync(searchString, sortOrder, pageNumber, pageSize);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAuctionBan(int id, string? reason)
    {
        await _adminService.ToggleAuctionBanAsync(id, reason);
        return RedirectToAction(nameof(Auctions));
    }
}