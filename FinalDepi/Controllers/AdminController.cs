using FinalDepi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinalDepi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // List all users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // List all orders
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders.Include(o => o.User).Include(o => o.OrderDetails).ThenInclude(od => od.Product).ToListAsync();
            return View(orders);
        }

        // List all products
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // List all categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // Add product (GET)
        public IActionResult AddProduct()
        {
            var categories = _context.Categories.ToList();
            if (!categories.Any())
            {
                TempData["ToastMessage"] = "Please add a category before adding a product.";
                return RedirectToAction("Categories");
            }
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
            return View(new Product());
        }

        // Add product (POST)
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product model)
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
            if (!categories.Any())
            {
                TempData["ToastMessage"] = "Please add a category before adding a product.";
                return RedirectToAction("Categories");
            }
            if (model.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Category is required.");
            }
            if (ModelState.IsValid)
            {
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                    model.ImagePath = "/images/products/" + uniqueFileName;
                }
                _context.Products.Add(model);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Product added successfully!";
                return RedirectToAction("Products");
            }
            return View(model);
        }

        // Edit product (GET)
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name");
            return View(product);
        }

        // Edit product (POST)
        [HttpPost]
        public async Task<IActionResult> EditProduct(Product model)
        {
            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null) return NotFound();
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.Stock = model.Stock;
                product.CategoryId = model.CategoryId;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                    product.ImagePath = "/images/products/" + uniqueFileName;
                }
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Product updated successfully!";
                return RedirectToAction("Products");
            }
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name");
            return View(model);
        }

        // Delete product
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Product deleted successfully!";
            }
            return RedirectToAction("Products");
        }

        // Add category (GET)
        public IActionResult AddCategory()
        {
            return View();
        }

        // Add category (POST)
        [HttpPost]
        public async Task<IActionResult> AddCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(model);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Category added successfully!";
                return RedirectToAction("Categories");
            }
            return View(model);
        }

        // Edit category (GET)
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // Edit category (POST)
        [HttpPost]
        public async Task<IActionResult> EditCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(model);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Category updated successfully!";
                return RedirectToAction("Categories");
            }
            return View(model);
        }

        // Delete category
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Category deleted successfully!";
            }
            return RedirectToAction("Categories");
        }

        // Edit user (GET)
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // Edit user (POST)
        [HttpPost]
        public async Task<IActionResult> EditUser(ApplicationUser model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.StreetAddress = model.StreetAddress;
            user.City = model.City;
            user.State = model.State;
            user.Country = model.Country;
            user.DateOfBirth = model.DateOfBirth;
            user.PhoneNumber = model.PhoneNumber;
            await _userManager.UpdateAsync(user);
            TempData["ToastMessage"] = "User updated successfully!";
            return RedirectToAction("Users");
        }

        // Delete user (GET)
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // Delete user (POST)
        [HttpPost, ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            await _userManager.DeleteAsync(user);
            TempData["ToastMessage"] = "User deleted successfully!";
            return RedirectToAction("Users");
        }
    }
} 