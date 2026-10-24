using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsEcommerceConsole.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ?Description { get; set; }
        public int StockQuantity { get; set; }
        public decimal Price { get; set; } 
        public bool IsActive { get; set; }

    }
}
