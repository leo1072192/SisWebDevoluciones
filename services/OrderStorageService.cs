using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OrderStorageService : IOrderStorageService
    {
        private readonly AppDbContext _context;

        public OrderStorageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveOrdersAsync(List<Order> orders)
        {
            foreach (var order in orders)
            {
                // Verificar si la orden ya existe en la base de datos
                var existingOrder = await _context.Orders
                    .AnyAsync(o => o.DocEntry == order.DocEntry);

                if (!existingOrder)
                {
                    // Si la orden no existe, agrégala junto con las líneas de documentos
                    var orderEntity = new Order
                    {
                        DocEntry = order.DocEntry,
                        DocNum = order.DocNum,
                        CardCode = order.CardCode,
                        DocDate = order.DocDate.ToUniversalTime(),
                        DocDueDate = order.DocDueDate.ToUniversalTime(),
                        DocumentLines = order.DocumentLines.Select(dl => new DocumentLine
                        {
                            ItemCode = dl.ItemCode,
                            ItemDescription = dl.ItemDescription,
                            Quantity = dl.Quantity,
                            ShipDate = dl.ShipDate,
                            Price = dl.Price,
                            PriceAfterVAT = dl.PriceAfterVAT,
                            Currency = dl.Currency,
                            Rate = dl.Rate,
                            DiscountPercent = dl.DiscountPercent,
                            WarehouseCode = dl.WarehouseCode,
                            ItemDetails = dl.ItemDetails
                        }).ToList()
                    };

                    _context.Orders.Add(orderEntity);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
