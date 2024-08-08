using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersApiService _ordersApiService;

        public OrdersController(IOrdersApiService ordersApiService)
        {
            _ordersApiService = ordersApiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(string cardCode)
        {
            try
            {
                var orders = await _ordersApiService.GetOrdersByCardCodeAsync(cardCode);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Loguea el error
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetOrdersALll()
        {
            try
            {
                var orders = await _ordersApiService.GetOrdersByCardCodeAsyncAll();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Loguea el error
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateOrderQuantity([FromBody] UpdateOrderQuantityDto updateOrderQuantityDto)
        {
            try
            {
                var result = await _ordersApiService.UpdateOrderQuantityAsync(updateOrderQuantityDto.OrderId, updateOrderQuantityDto.LineId, updateOrderQuantityDto.NewQuantity);
                if (result)
                {
                    return Ok(new { message = "Order quantity updated successfully." });
                }
                else
                {
                    return NotFound(new { message = "Order or document line not found." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
