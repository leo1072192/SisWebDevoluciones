using Application.Interfaces;
using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Infrastructure.Persistence.Contexts;
namespace Infrastructure.Services
{
    public class OrdersApiService : IOrdersApiService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IOrderRepository _orderRepository;
        private readonly string _baseUrl = "https://181.39.149.2:50000/b1s/v1/";

        public OrdersApiService(AppDbContext context,HttpClient httpClient, IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        //public async Task<List<OrderDto>> GetOrdersByCardCodeAsync(string cardCode)
        //{
        //    var endpoint = $"Orders?$select=DocEntry,DocNum,CardCode,DocDate,DocDueDate,DocumentLines&$filter=CardCode eq '{cardCode}'&$orderby=CreationDate";

        //    var accessToken = await SapAuthenticationHelper.GetAccessTokenAsync(_httpClient);

        //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //    var response = await _httpClient.GetAsync(endpoint);
        //    response.EnsureSuccessStatusCode();

        //    var responseContent = await response.Content.ReadAsStringAsync();

        //    // Parse the JSON response
        //    using (JsonDocument doc = JsonDocument.Parse(responseContent))
        //    {
        //        List<OrderDto> orderDtos = new List<OrderDto>();

        //        // Get the root element
        //        JsonElement root = doc.RootElement;

        //        // Check if "value" property exists and is an array
        //        if (root.TryGetProperty("value", out JsonElement valueElement) && valueElement.ValueKind == JsonValueKind.Array)
        //        {
        //            // Iterate over each element in the array
        //            foreach (JsonElement orderElement in valueElement.EnumerateArray())
        //            {
        //                // Map JSON properties to OrderDto
        //                OrderDto orderDto = new OrderDto
        //                {
        //                    DocEntry = orderElement.GetProperty("DocEntry").GetInt32(),
        //                    DocNum = orderElement.GetProperty("DocNum").GetInt32(),
        //                    CardCode = orderElement.GetProperty("CardCode").GetString(),
        //                    DocDate = orderElement.GetProperty("DocDate").GetDateTime(),
        //                    DocDueDate = orderElement.GetProperty("DocDueDate").GetDateTime(),
        //                    DocumentLines = new List<DocumentLineDto>()
        //                };

        //                // Process DocumentLines array
        //                if (orderElement.TryGetProperty("DocumentLines", out JsonElement documentLinesElement) && documentLinesElement.ValueKind == JsonValueKind.Array)
        //                {
        //                    foreach (JsonElement documentLineElement in documentLinesElement.EnumerateArray())
        //                    {
        //                        DocumentLineDto documentLineDto = new DocumentLineDto
        //                        {
        //                            ItemCode = documentLineElement.GetProperty("ItemCode").GetString(),
        //                            ItemDescription = documentLineElement.GetProperty("ItemDescription").GetString(),
        //                            Quantity = documentLineElement.GetProperty("Quantity").GetDouble(),
        //                            ShipDate = documentLineElement.GetProperty("ShipDate").GetDateTime(),
        //                            Price = documentLineElement.GetProperty("Price").GetDouble(),
        //                            PriceAfterVAT = documentLineElement.GetProperty("PriceAfterVAT").GetDouble(),
        //                            Currency = documentLineElement.GetProperty("Currency").GetString(),
        //                            Rate = documentLineElement.GetProperty("Rate").GetDouble(),
        //                            DiscountPercent = documentLineElement.GetProperty("DiscountPercent").GetDouble(),
        //                            WarehouseCode = documentLineElement.GetProperty("WarehouseCode").GetString(),
        //                            ItemDetails = documentLineElement.GetProperty("ItemDetails").GetString()
        //                        };

        //                        orderDto.DocumentLines.Add(documentLineDto);
        //                    }
        //                }

        //                orderDtos.Add(orderDto);
        //            }
        //        }

        //        return orderDtos;
        //    }
        //}
        public async Task<List<OrderDto>> GetOrdersByCardCodeAsync(string cardCode)
        {
            // Verificar si el código de tarjeta es válido
            if (string.IsNullOrEmpty(cardCode))
            {
                throw new ArgumentException("El código de tarjeta no puede ser nulo o vacío.", nameof(cardCode));
            }

            // Verificar si el contexto de base de datos está inicializado
            if (_context == null)
            {
                throw new InvalidOperationException("El contexto de la base de datos no está inicializado.");
            }

            // Obtener órdenes de la base de datos PostgreSQL
            var orders = await _context.Orders
                .Include(o => o.DocumentLines)
                .Where(o => o.CardCode != null && o.CardCode == cardCode)
                .ToListAsync();

            // Mapear a OrderDto
            var orderDtos = orders.Select(o => new OrderDto
            {
                DocEntry = o.DocEntry,
                DocNum = o.DocNum,
                CardCode = o.CardCode,
                DocDate = o.DocDate,
                DocDueDate = o.DocDueDate,
                DocumentLines = o.DocumentLines != null ? o.DocumentLines.Select(dl => new DocumentLineDto
                {
                    Id=dl.Id,
                    ItemCode = dl.ItemCode,
                    ItemDescription = dl.ItemDescription,
                    Quantity = dl.Quantity,
                    ShipDate = dl.ShipDate,
                    Price = dl.Price,
                    PriceAfterVAT = dl.PriceAfterVAT,
                    Currency = dl.Currency,
                    Rate = dl.Rate,
                    DiscountPercent = dl.DiscountPercent,
                    WarehouseCode = dl.WarehouseCode,
                    ItemDetails = dl.ItemDetails,
                    OrderId=dl.OrderId,
                }).ToList() : new List<DocumentLineDto>()
            }).ToList();

            return orderDtos;
        }


        public async Task<List<Order>> GetOrdersByCardCodeAsyncAll()
        {
            var endpoint = "Orders?$select=DocEntry,DocNum,CardCode,DocDate,DocDueDate,DocumentLines &$filter = CardCode eq '{cardCode}' &$orderby = CreationDate";

            var accessToken = await SapAuthenticationHelper.GetAccessTokenAsync(_httpClient);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse the JSON response
            using (JsonDocument doc = JsonDocument.Parse(responseContent))
            {
                List<Order> orderDtos = new List<Order>();

                // Get the root element
                JsonElement root = doc.RootElement;

                // Check if "value" property exists and is an array
                if (root.TryGetProperty("value", out JsonElement valueElement) && valueElement.ValueKind == JsonValueKind.Array)
                {
                    // Iterate over each element in the array
                    foreach (JsonElement orderElement in valueElement.EnumerateArray())
                    {
                        // Map JSON properties to OrderDto
                        Order order = new Order
                        {
                            DocEntry = orderElement.GetProperty("DocEntry").GetInt32(),
                            DocNum = orderElement.GetProperty("DocNum").GetInt32(),
                            CardCode = orderElement.GetProperty("CardCode").GetString(),
                            DocDate = orderElement.GetProperty("DocDate").GetDateTime(),
                            DocDueDate = orderElement.GetProperty("DocDueDate").GetDateTime(),
                            DocumentLines = new List<DocumentLine>()
                        };

                        // Process DocumentLines array
                        if (orderElement.TryGetProperty("DocumentLines", out JsonElement documentLinesElement) && documentLinesElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement documentLineElement in documentLinesElement.EnumerateArray())
                            {
                                DocumentLine documentLineDto = new DocumentLine
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

                                order.DocumentLines.Add(documentLineDto);
                            }
                        }

                        orderDtos.Add(order);
                    }
                }

                return orderDtos;
            }


        }
        public async Task<bool> UpdateOrderQuantityAsync(int orderId, int lineId, int newQuantity)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                Console.WriteLine("Order not found.");
                return false;
            }

            var line = order.DocumentLines.FirstOrDefault(dl => dl.Id == lineId);
            if (line == null)
            {
                Console.WriteLine("Document line not found.");
                return false;
            }

            line.Quantity = newQuantity;
            await _orderRepository.UpdateOrderAsync(order);

            return true;
        }
    }
}
