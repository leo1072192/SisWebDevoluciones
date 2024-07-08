using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services
{
    public static class SapAuthenticationHelper
    {
        public static async Task<string> GetAccessTokenAsync(HttpClient httpClient)
        {
            string accessToken = null;

            // Check if access token is already cached
            if (!string.IsNullOrEmpty(accessToken))
            {
                return accessToken;
            }

            // Otherwise, fetch new access token
            var loginEndpoint = "Login";
            var loginPayload = new
            {
                CompanyDB = "SBO_EC_SL_TEST",
                Password = "2022",
                UserName = "SISTEMAS2"
            };

            var jsonPayload = JsonSerializer.Serialize(loginPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(loginEndpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);
                accessToken = loginResponse.SessionId; // Assuming SessionId is the token received
                return accessToken;
            }
            else
            {
                throw new HttpRequestException($"Failed to obtain access token: {response.StatusCode}");
            }
        }
    }
}
