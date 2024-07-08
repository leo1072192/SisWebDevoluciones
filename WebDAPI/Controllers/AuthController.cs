using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISapB1Service _sapB1Service;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService; // Asegúrate de tener esto inyectado correctamente

        public AuthController(ISapB1Service sapB1Service, IUserRepository userRepository, IJwtService jwtService)
        {
            _sapB1Service = sapB1Service;
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Verificar y autenticar al usuario utilizando SAP B1 o base de datos local
            var customerInfo = await _sapB1Service.VerifyAndAuthenticateUserAsync(request.CardCode, request.Password);
            if (customerInfo != null)
            {
                // Generar token JWT
                var token = _jwtService.GenerateToken(customerInfo.CardCode, request.Password);

                // Devolver el customerInfo y el token como parte de la respuesta
                return Ok(new { CustomerInfo = customerInfo, Token = token });
            }
            return Unauthorized("Invalid credentials");
        }
    }

    public class LoginRequest
    {
        public string CardCode { get; set; }
        public string Password { get; set; }
    }
}
