using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
   
        public class OrderDto
        {
            public int DocEntry { get; set; }
            public int DocNum { get; set; }
            public string CardCode { get; set; }
            public DateTime DocDate { get; set; }
            public DateTime DocDueDate { get; set; }
            public List<DocumentLineDto> DocumentLines { get; set; }
        }

        public class DocumentLineDto
        {
            public string ItemCode { get; set; }
            public string ItemDescription { get; set; }
            public double Quantity { get; set; }
            public DateTime ShipDate { get; set; }
            public double Price { get; set; }
            public double PriceAfterVAT { get; set; }
            public string Currency { get; set; }
            public double Rate { get; set; }
            public double DiscountPercent { get; set; }
            public string WarehouseCode { get; set; }
            public string ItemDetails { get; set; }
        }
   
}
