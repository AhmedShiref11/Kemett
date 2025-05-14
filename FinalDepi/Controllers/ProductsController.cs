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
        public IActionResult Index(string sortOrder, int? categoryId)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Filter by category if specified
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            // Get all categories for the filter
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.SelectedCategory = categoryId;

            return View(products.ToList());
        }

        // Display categories page
        public IActionResult Categories()
        {
            var categories = _context.Categories
                .Include(c => c.Products)
                .ToList();
            return View(categories);
        }

        public async Task<IActionResult> Search(string query)
        {
            var products = string.IsNullOrWhiteSpace(query)
                ? new List<Product>()
                : await _context.Products
                    .Where(p => p.Name.Contains(query))
                    .ToListAsync();
            ViewBag.Query = query;
            return View(products);
        }
    }
} 