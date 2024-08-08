using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class ReturnRequestDto
    {

        public string CardCode { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public List<DocumentLine2Dto> DocumentLines { get; set; }
        public string Estado { get; set; }
        //public DateTime FechaInsercion { get; set; }
        //public string CreatedBy { get; set; }
        //public string UpdatedBy { get; set; }
        //public DateTime CreatedAt { get; set; }
        //public DateTime UpdatedAt { get; set; }
    }

    public class DocumentLine2Dto
    {
        public int OrderId { get; set; }  // Foreign key
        public int Id { get; set; }  // Foreign key
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public string WarehouseCode { get; set; }

        public int devolucionQuantity { get; set; }

       
    }
}
