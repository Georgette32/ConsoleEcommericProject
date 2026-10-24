using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsEcommerceConsole.Models
{
    public class Order
    {public int OrderId { get; set; }
        public int UserId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public string Username { get; set; } = string.Empty;
    }
}
