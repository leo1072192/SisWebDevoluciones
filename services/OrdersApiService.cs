using Application.Interfaces;
using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Services;

namespace Infrastructure.Services
{
    public class OrdersApiService : IOrdersApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://181.39.149.2:50000/b1s/v1/";

        public OrdersApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<OrderDto>> GetOrdersByCardCodeAsync(string cardCode)
        {
            var endpoint = $"Orders?$select=DocEntry,DocNum,CardCode,DocDate,DocDueDate,DocumentLines&$filter=CardCode eq '{cardCode}'&$orderby=CreationDate";

            var accessToken = await SapAuthenticationHelper.GetAccessTokenAsync(_httpClient);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse the JSON response
            using (JsonDocument doc = JsonDocument.Parse(responseContent))
            {
                List<OrderDto> orderDtos = new List<OrderDto>();

                // Get the root element
                JsonElement root = doc.RootElement;

                // Check if "value" property exists and is an array
                if (root.TryGetProperty("value", out JsonElement valueElement) && valueElement.ValueKind == JsonValueKind.Array)
                {
                    // Iterate over each element in the array
                    foreach (JsonElement orderElement in valueElement.EnumerateArray())
                    {
                        // Map JSON properties to OrderDto
                        OrderDto orderDto = new OrderDto
                        {
                            DocEntry = orderElement.GetProperty("DocEntry").GetInt32(),
                            DocNum = orderElement.GetProperty("DocNum").GetInt32(),
                            CardCode = orderElement.GetProperty("CardCode").GetString(),
                            DocDate = orderElement.GetProperty("DocDate").GetDateTime(),
                            DocDueDate = orderElement.GetProperty("DocDueDate").GetDateTime(),
                            DocumentLines = new List<DocumentLineDto>()
                        };

                        // Process DocumentLines array
                        if (orderElement.TryGetProperty("DocumentLines", out JsonElement documentLinesElement) && documentLinesElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement documentLineElement in documentLinesElement.EnumerateArray())
                            {
                                DocumentLineDto documentLineDto = new DocumentLineDto
                                {
                                    ItemCode = documentLineElement.GetProperty("ItemCode").GetString(),
                                    ItemDescription = documentLineElement.GetProperty("ItemDescription").GetString(),
                                    Quantity = documentLineElement.GetProperty("Quantity").GetDouble(),
                                    ShipDate = documentLineElement.GetProperty("ShipDate").GetDateTime(),
                                    Price = documentLineElement.GetProperty("Price").GetDouble(),
                                    PriceAfterVAT = documentLineElement.GetProperty("PriceAfterVAT").GetDouble(),
                                    Currency = documentLineElement.GetProperty("Currency").GetString(),
                                    Rate = documentLineElement.GetProperty("Rate").GetDouble(),
                                    DiscountPercent = documentLineElement.GetProperty("DiscountPercent").GetDouble(),
                                    WarehouseCode = documentLineElement.GetProperty("WarehouseCode").GetString(),
                                    ItemDetails = documentLineElement.GetProperty("ItemDetails").GetString()
                                };

                                orderDto.DocumentLines.Add(documentLineDto);
                            }
                        }

                        orderDtos.Add(orderDto);
                    }
                }

                return orderDtos;
            }
        }
    }
}
