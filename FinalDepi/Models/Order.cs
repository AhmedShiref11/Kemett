using System;
using System.Collections.Generic;
namespace FinalDepi.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}