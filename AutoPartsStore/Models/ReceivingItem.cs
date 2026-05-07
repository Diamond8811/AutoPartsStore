using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsStore.Models
{
    public class ReceivingItem
    {
        public Products Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
