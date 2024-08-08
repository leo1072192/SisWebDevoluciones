using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UpdateOrderQuantityDto
    {
        public int OrderId { get; set; }
        public int LineId { get; set; }
        public int NewQuantity { get; set; }
    }
}
