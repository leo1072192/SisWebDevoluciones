using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BCrypt.Net;
using Microsoft.Extensions.DependencyInjection;
using Services;
namespace Infrastructure.Services
{
    public class SapB1Service : ISapB1Service
    {
        private readonly HttpClient _httpClient;
        private readonly IUserRepository _userRepository;
        private string _accessToken;

        public SapB1Service(HttpClient httpClient, IUserRepository userRepository)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.BaseAddress = new Uri("https://181.39.149.2:50000/b1s/v1/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<SapUserDto> VerifyAndAuthenticateUserAsync(string cardCode, string password)
        {
            try
            {
                // Verificar en la base de datos local primero
                var userInDb = await _userRepository.GetUserByCardCodeAsync(cardCode);
                if (userInDb != null)
                {
                    // Ejemplo de validación de contraseña (esto puede variar según cómo esté implementado tu sistema de autenticación)
                    if (BCrypt.Net.BCrypt.Verify(password, userInDb.Password))
                    {
                        // Si la contraseña es correcta, usar la API para traer los datos del cliente
                        var accessToken = await SapAuthenticationHelper.GetAccessTokenAsync(_httpClient);

                        var businessPartnerEndpoint2 = $"BusinessPartners('{cardCode}')?$select=CardCode,CardName,CardType,GroupCode,Address,Phone1,ContactPerson,Notes,PayTermsGrpCode,CreditLimit,MaxCommitment,FederalTaxID,Cellular,EmailAddress,CardForeignName,DebitorAccount";
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                        var response2 = await _httpClient.GetAsync(businessPartnerEndpoint2);
                        response2.EnsureSuccessStatusCode();

                        var responseContent2 = await response2.Content.ReadAsStringAsync();
                        var sapUserDto = JsonSerializer.Deserialize<SapUserDto>(responseContent2);
                        // Actualizar el DTO con el Id y Role de la base de datos
                        sapUserDto.Id = userInDb.Id.ToString(); // Convertir a string si es necesario
                        sapUserDto.Role = userInDb.Role;
                        return sapUserDto;
                    }
                    else
                    {
                        return null;
                    }
                }

                // Si no está en la base de datos local, verificar en SAP B1
                var accessToken2 = await SapAuthenticationHelper.GetAccessTokenAsync(_httpClient);

                var businessPartnerEndpoint = $"BusinessPartners('{cardCode}')?$select=CardCode,CardName,CardType,GroupCode,Address,Phone1,ContactPerson,Notes,PayTermsGrpCode,CreditLimit,MaxCommitment,FederalTaxID,Cellular,EmailAddress,CardForeignName,DebitorAccount";
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken2);

                var response = await _httpClient.GetAsync(businessPartnerEndpoint);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var SapUserDto = JsonSerializer.Deserialize<SapUserDto>(responseContent);

                // Insertar en la base de datos local
                var newUser = new User
                {
                    CardCode = SapUserDto.CardCode,
                    CardName = SapUserDto.CardName,
                    FederalTaxID = SapUserDto.FederalTaxID,
                    Password = BCrypt.Net.BCrypt.HashPassword(password), //
                    Role = "User" // Definir el rol adecuado según tu lógica
                };

                await _userRepository.AddUserAsync(newUser);

                return SapUserDto; // Usuario autenticado y registrado correctamente
            }
            catch (HttpRequestException ex)
            {
                // Maneja la excepción de manera adecuada
                Console.WriteLine($"Error in VerifyAndAuthenticateUserAsync: {ex.Message}");
                return null;
            }
        }
    }

}