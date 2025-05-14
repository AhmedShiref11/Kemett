using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalDepi.Models;
using Microsoft.AspNetCore.Authorization;

namespace FinalDepi.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string query)
        {
            if (string.IsNullOrEmpty(query))
                return RedirectToAction("Index", "Home");

            var currentPath = HttpContext.Request.Path.Value?.ToLower();

            if (currentPath?.Contains("/admin/users") == true)
            {
                var users = await _context.Users
                    .Where(u => u.UserName.Contains(query) || 
                               u.Email.Contains(query) || 
                               u.FirstName.Contains(query) || 
                               u.LastName.Contains(query))
                    .ToListAsync();
                return View("Users", users);
            }
            else if (currentPath?.Contains("/admin/products") == true)
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Name.Contains(query) || 
                               p.Description.Contains(query) || 
                               p.Category.Name.Contains(query))
                    .ToListAsync();
                return View("~/Views/Products/Index.cshtml", products);
            }
            else if (currentPath?.Contains("/admin/orders") == true)
            {
                var orders = await _context.Orders
                    .Include(o => o.User)
                    .Where(o => o.OrderId.ToString().Contains(query) || 
                               o.User.UserName.Contains(query) || 
                               o.PaymentStatus.Contains(query))
                    .ToListAsync();
                return View("Orders", orders);
            }
            else
            {
                // Default product search for main site
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Name.Contains(query) || 
                               p.Description.Contains(query) || 
                               p.Category.Name.Contains(query))
                    .ToListAsync();
                return View("~/Views/Products/Index.cshtml", products);
            }
        }
    }
} 