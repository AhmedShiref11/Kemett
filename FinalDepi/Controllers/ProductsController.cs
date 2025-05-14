using FinalDepi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FinalDepi.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Search(string query)
        {
            var products = string.IsNullOrWhiteSpace(query)
                ? new List<Product>()
                : await _context.Products
                    .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                    .ToListAsync();
            ViewBag.Query = query;
            return View(products);
        }
    }
} 