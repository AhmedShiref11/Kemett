using System.ComponentModel.DataAnnotations;
using FinalDepi.Models;

namespace FinalDepi.Models
{
    public class ShoppingCartItem
    {
        [Key]
        public int CartItemId { get; set; }
        public int ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
} 