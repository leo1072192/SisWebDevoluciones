// DevolucionesController.cs
using System;
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
    public class DevolucionesController : ControllerBase
    {
        private readonly IDevolucionesApiService _devolucionesApiService;

        public DevolucionesController(IDevolucionesApiService devolucionesApiService)
        {
            _devolucionesApiService = devolucionesApiService;
        }

        [HttpPost]
        public async Task<IActionResult> InsertarDevolucion(DevolucionDto devolucion)
        {
            try
            {
                var resultado = await _devolucionesApiService.InsertarDevolucionAsync(devolucion);
                if (resultado)
                    return Ok(new { message = "Devolución insertada correctamente" });
                else
                    return BadRequest(new { message = "Error al insertar devolución" });
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
