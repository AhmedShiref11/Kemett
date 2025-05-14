using FinalDepi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;

namespace FinalDepi.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PayPalConfig _paypalConfig;

        public PaymentController(ApplicationDbContext context, PayPalConfig paypalConfig)
        {
            _context = context;
            _paypalConfig = paypalConfig;
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.PayPalClientId = _paypalConfig.ClientId;
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDirectOrder()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cart = await _context.ShoppingCarts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                {
                    return Json(new { success = false, message = "Cart is empty" });
                }

                // Update product stock
                foreach (var item in cart.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                        if (product.Stock < 0)
                        {
                            return Json(new { success = false, message = $"Insufficient stock for {product.Name}" });
                        }
                    }
                }

                // Create order
                var order = new FinalDepi.Models.Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity),
                    PaymentMethod = "Direct",
                    PaymentStatus = "Paid",
                    OrderDetails = cart.Items.Select(item => new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                };

                _context.Orders.Add(order);
                _context.ShoppingCarts.Remove(cart);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Order processed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayPalOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return BadRequest("Cart is empty");
            }

            var environment = new SandboxEnvironment(_paypalConfig.ClientId, _paypalConfig.ClientSecret);
            var client = new PayPalHttpClient(environment);

            var order = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = "USD",
                            Value = cart.Items.Sum(i => i.Price * i.Quantity).ToString("0.00")
                        },
                        Items = cart.Items.Select(item => new Item
                        {
                            Name = item.Product.Name,
                            Quantity = item.Quantity.ToString(),
                            UnitAmount = new Money
                            {
                                CurrencyCode = "USD",
                                Value = item.Price.ToString("0.00")
                            }
                        }).ToList()
                    }
                }
            };

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(order);

            try
            {
                var response = await client.Execute(request);
                var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                return Json(new { orderId = result.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CapturePayPalOrder(string orderId)
        {
            var environment = new SandboxEnvironment(_paypalConfig.ClientId, _paypalConfig.ClientSecret);
            var client = new PayPalHttpClient(environment);

            var request = new OrdersCaptureRequest(orderId);
            request.Prefer("return=representation");

            try
            {
                var response = await client.Execute(request);
                var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                if (result.Status == "COMPLETED")
                {
                    // Create order in database
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cart = await _context.ShoppingCarts
                        .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    // Update product stock
                    foreach (var item in cart.Items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Stock -= item.Quantity;
                            if (product.Stock < 0)
                            {
                                return BadRequest($"Insufficient stock for {product.Name}");
                            }
                        }
                    }

                    var order = new FinalDepi.Models.Order
                    {
                        UserId = userId,
                        OrderDate = DateTime.UtcNow,
                        TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity),
                        PaymentMethod = "PayPal",
                        PaymentStatus = "Paid",
                        OrderDetails = cart.Items.Select(item => new OrderDetail
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        }).ToList()
                    };

                    _context.Orders.Add(order);
                    _context.ShoppingCarts.Remove(cart);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }

                return BadRequest("Payment not completed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
} 