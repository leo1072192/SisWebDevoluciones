// Devolucion.cs

using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Devolucion
    {
        public int Id { get; set; }
        public string CardCode { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public string Estado { get; set; } // Nuevo campo para el estado de la devolución
        public ICollection<DevolucionLinea> DocumentLines { get; set; }
    }

    public class DevolucionLinea
    {
        public int Id { get; set; }
        public string ItemCode { get; set; }
        public double Quantity { get; set; }
        public string WarehouseCode { get; set; }
        public int DevolucionId { get; set; }
        public Devolucion Devolucion { get; set; }
    }
}
