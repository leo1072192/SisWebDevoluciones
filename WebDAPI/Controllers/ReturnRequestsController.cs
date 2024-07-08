using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReturnRequestsController : ControllerBase
    {
        private readonly IReturnRequestService _returnRequestService;

        public ReturnRequestsController(IReturnRequestService returnRequestService)
        {
            _returnRequestService = returnRequestService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> InsertarReturnRequest(ReturnRequestDto returnRequest)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                returnRequest.CreatedBy = userName;
                returnRequest.UpdatedBy = userName;
                returnRequest.FechaInsercion = DateTime.UtcNow;
                returnRequest.CreatedAt = DateTime.UtcNow;
                returnRequest.UpdatedAt = DateTime.UtcNow;

                var resultado = await _returnRequestService.InsertarReturnRequestAsync(returnRequest);
                if (resultado)
                    return Ok(new { message = "Return request inserted successfully" });
                else
                    return BadRequest(new { message = "Error inserting return request" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarReturnRequest(int id, ReturnRequestDto returnRequest)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                returnRequest.UpdatedBy = userName;

                var resultado = await _returnRequestService.ActualizarReturnRequestAsync(id, returnRequest);
                if (resultado)
                    return Ok(new { message = "Return request updated successfully" });
                else
                    return NotFound(new { message = "Return request not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
