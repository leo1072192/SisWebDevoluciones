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
    }
}
