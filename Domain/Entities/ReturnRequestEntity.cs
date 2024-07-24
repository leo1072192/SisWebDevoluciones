using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class ReturnRequestEntity
    {
        public int Id { get; set; } // Asegúrate de tener una clave primaria
        public string CardCode { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public List<DocumentLineEntity> DocumentLines { get; set; }
        public string Estado { get; set; }
        public DateTime FechaInsercion { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DocumentLineEntity
    {
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public string WarehouseCode { get; set; }

        public int devolucionQuantity { get; set; }
    }
}
