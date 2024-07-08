using Application.Interfaces;
using Infrastructure.Persistence.Contexts;
using Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;

namespace Infrastructure.Shared
{
    public static class ServiceExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Registro de IUserRepository
            services.AddScoped<IUserRepository, UserRepository>();

            // Configuración de HttpClient para ISapB1Service
            services.AddHttpClient<ISapB1Service, SapB1Service>(client =>
            {
                client.BaseAddress = new Uri("https://181.39.149.2:50000/b1s/v1/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });

            // Configuración de HttpClient para IOrdersApiService
            services.AddHttpClient<IOrdersApiService, OrdersApiService>(client =>
            {
                client.BaseAddress = new Uri("https://181.39.149.2:50000/b1s/v1/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });

            // Configuración de HttpClient para IDevolucionesApiService
            services.AddHttpClient<IDevolucionesApiService, DevolucionesApiService>(client =>
            {
                client.BaseAddress = new Uri("https://181.39.149.2:50000/b1s/v1/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });

            // Configuración de HttpClient para IReturnRequestService
            services.AddHttpClient<IReturnRequestService, ReturnRequestService>(client =>
            {
                client.BaseAddress = new Uri("https://181.39.149.2:50000/b1s/v1/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });

            // Registro del JwtService
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IReturnRequestService, ReturnRequestService>();

            // Registro de otros servicios de infraestructura...
        }
    }
}
