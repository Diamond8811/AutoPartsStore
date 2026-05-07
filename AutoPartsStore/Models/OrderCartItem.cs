using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsStore.Models
{
    public class OrderCartItem
    {
        public Products Product { get; set; }
        public int Quantity { get; set; }
    }
}
