using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigin")]
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

                //returnRequest.CreatedBy = userName;
                //returnRequest.UpdatedBy = userName;
                //returnRequest.FechaInsercion = DateTime.UtcNow;
                //returnRequest.CreatedAt = DateTime.UtcNow;
                //returnRequest.UpdatedAt = DateTime.UtcNow;

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


        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarReturnRequest(int id, ReturnRequestDto returnRequest)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                //returnRequest.UpdatedBy = userName;

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

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAllReturnRequests()
        {
            try
            {
                var returnRequests = await _returnRequestService.GetAllReturnRequestsAsync();
                return Ok(returnRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("byCardCode/{cardCode}")]
        [EnableCors("AllowSpecificOrigin")]
        public async Task<IActionResult> GetReturnRequestsByCardCode(string cardCode)
        {
            try
            {
                var userCardCode = User.FindFirst(ClaimTypes.Name)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && cardCode != userCardCode)
                {
                    return Forbid(); // Retorna un código 403 Forbidden si no está autorizado
                }

                var returnRequests = await _returnRequestService.GetReturnRequestsByCardCodeAsync(cardCode);
                return Ok(returnRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }


    }
}
