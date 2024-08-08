using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Application.Interfaces;
using Infrastructure.Services;
namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduledTasksController : ControllerBase
    {
        private readonly IOrderStorageService _scheduledTasks;
        private readonly IOrdersApiService _ordersApiService;

        public ScheduledTasksController(IOrderStorageService scheduledTasks, IOrdersApiService ordersApiService)
        {
            _scheduledTasks = scheduledTasks;
            _ordersApiService = ordersApiService;
        }

        [HttpPost("fetch-orders")]
        public async Task<IActionResult> FetchOrders()
        {
            try
            {
               
                var orders = await _ordersApiService.GetOrdersByCardCodeAsyncAll();
                await _scheduledTasks.SaveOrdersAsync(orders);
                return Ok("Orders fetched and stored successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
