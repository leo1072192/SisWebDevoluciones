using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string CardCode { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public List<DocumentLine> DocumentLines { get; set; }
    }

    public class DocumentLine
    {
        public int Id { get; set; }
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
        public int OrderId { get; set; }  // Foreign key
        public Order Order { get; set; }  // Navigation property
    }
}
