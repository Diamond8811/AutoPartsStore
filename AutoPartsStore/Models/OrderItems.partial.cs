using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsStore.Models
{
    public partial class OrderItems
    {
        public override string ToString()
        {
            return Products?.Name ?? OrderItemId.ToString();
        }
    }
}
