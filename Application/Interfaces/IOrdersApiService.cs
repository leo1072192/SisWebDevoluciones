using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
namespace Application.Interfaces
{
    public interface IOrdersApiService
    {
        Task<List<OrderDto>> GetOrdersByCardCodeAsync(string cardCode);
        Task<List<Order>> GetOrdersByCardCodeAsyncAll();
        Task<bool> UpdateOrderQuantityAsync(int orderId, int lineId, int newQuantity);

    }
}
