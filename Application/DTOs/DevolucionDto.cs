// DevolucionDto.cs
using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class DevolucionDto
    {
        public string CardCode { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public List<DevolucionLineDto> DocumentLines { get; set; }
    }

    public class DevolucionLineDto
    {
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public string WarehouseCode { get; set; }
        public int devolucionQuantity { get; set; }
    }
}
