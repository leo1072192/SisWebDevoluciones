// DevolucionesApiService.cs
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Services;

namespace Infrastructure.Services
{
    public class DevolucionesApiService : IDevolucionesApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://181.39.149.2:50000/b1s/v1/";

        public DevolucionesApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> InsertarDevolucionAsync(DevolucionDto devolucion)
        {
            try
            {
                var endpoint = "ReturnRequest";

                var accessToken = await SapAuthenticationHelper.GetAccessTokenAsync(_httpClient);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var jsonContent = JsonSerializer.Serialize(devolucion);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, httpContent);
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                // Loguea el error
                Console.WriteLine($"Error al insertar devolución: {ex.Message}");
                return false;
            }
        }
    }
}
