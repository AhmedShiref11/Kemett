using System.Collections.Generic;
using FinalDepi.Models;
using System.ComponentModel.DataAnnotations;

namespace FinalDepi.Models
{
    public class ShoppingCart
    {
        [Key]
        public int CartId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<ShoppingCartItem> Items { get; set; }
        public decimal TotalPrice { get; set; }
    }
}