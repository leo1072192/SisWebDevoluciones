using System;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Infrastructure.Services
{
    public class ReturnRequestService : IReturnRequestService
    {
        private readonly AppDbContext _context;
        private readonly IDevolucionesApiService _devolucionesApiService;

        public ReturnRequestService(AppDbContext context, IDevolucionesApiService devolucionesApiService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _devolucionesApiService = devolucionesApiService ?? throw new ArgumentNullException(nameof(devolucionesApiService));
        }

        public async Task<bool> InsertarReturnRequestAsync(ReturnRequestDto returnRequest)
        {
            try
            {
                var documentLines = returnRequest.DocumentLines.Select(dl => new DocumentLineEntity
                {
                    ItemCode = dl.ItemCode,
                    Quantity = dl.Quantity,
                    WarehouseCode = dl.WarehouseCode,
                    devolucionQuantity=dl.devolucionQuantity,
                }).ToList();

                var newReturnRequest = new ReturnRequestEntity
                {
                    CardCode = returnRequest.CardCode,
                    DocDate = returnRequest.DocDate.ToUniversalTime(),
                    DocDueDate = returnRequest.DocDueDate.ToUniversalTime(),
                    DocumentLines = documentLines,
                    Estado = returnRequest.Estado ?? "Pendiente",
                    FechaInsercion = DateTime.UtcNow, // Establece la fecha de inserción actual
                    CreatedBy = returnRequest.CardCode, // Establece el creador por defecto
                    UpdatedBy = returnRequest.CardCode, // Establece el actualizador por defecto
                    CreatedAt = DateTime.UtcNow, // Establece la fecha de creación actual
                    UpdatedAt = DateTime.UtcNow  // Establece la fecha de actualización actual
                    //FechaInsercion = DateTime.SpecifyKind(returnRequest.FechaInsercion, DateTimeKind.Utc),
                    //CreatedBy = returnRequest.CreatedBy,
                    //UpdatedBy = returnRequest.UpdatedBy,
                    //CreatedAt = DateTime.SpecifyKind(returnRequest.CreatedAt, DateTimeKind.Utc),
                    //UpdatedAt = DateTime.SpecifyKind(returnRequest.UpdatedAt, DateTimeKind.Utc),
                };

                _context.ReturnRequests.Add(newReturnRequest);
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Error al insertar ReturnRequest (DB): {dbEx.InnerException?.Message ?? dbEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar ReturnRequest: {ex}");
                throw;
            }
        }

        public async Task<bool> ActualizarReturnRequestAsync(int id, ReturnRequestDto returnRequest)
        {
            try
            {
                var existingRequest = await _context.ReturnRequests.FindAsync(id);
                if (existingRequest == null)
                {
                    return false;
                }

                existingRequest.CardCode = returnRequest.CardCode;
                existingRequest.DocDate = returnRequest.DocDate.ToUniversalTime();
                existingRequest.DocDueDate = returnRequest.DocDueDate.ToUniversalTime();
                existingRequest.DocumentLines = returnRequest.DocumentLines.Select(dl => new DocumentLineEntity
                {
                    ItemCode = dl.ItemCode,
                    Quantity = dl.Quantity,
                    WarehouseCode = dl.WarehouseCode,
                    devolucionQuantity = dl.devolucionQuantity,
                }).ToList();
                existingRequest.Estado = returnRequest.Estado ?? existingRequest.Estado;
                existingRequest.FechaInsercion = existingRequest.FechaInsercion.ToUniversalTime(); // Convertir a UTC
                existingRequest.CreatedBy = existingRequest.CreatedBy; // Mantener el creador original
                existingRequest.UpdatedBy = existingRequest.CreatedBy; // Actualizar el actualizador
                existingRequest.CreatedAt = existingRequest.CreatedAt.ToUniversalTime(); // Convertir a UTC
                existingRequest.UpdatedAt = DateTime.UtcNow;  // Establecer la fecha de actualización actual

                _context.ReturnRequests.Update(existingRequest);
                var result = await _context.SaveChangesAsync();

                if (existingRequest.Estado == "Aprobado")
                {
                    var devolucionDto = new DevolucionDto
                    {
                        CardCode = existingRequest.CardCode,
                        DocDate = existingRequest.DocDate,
                        DocDueDate = existingRequest.DocDueDate,
                        DocumentLines = existingRequest.DocumentLines.Select(dl => new DevolucionLineDto
                        {
                            ItemCode = dl.ItemCode,
                            Quantity = dl.devolucionQuantity,
                            WarehouseCode = dl.WarehouseCode,
                            devolucionQuantity = dl.devolucionQuantity,
                        }).ToList()
                    };
                    Console.Write("AAAA");
                    Console.Write(devolucionDto);
                    var apiResult = await _devolucionesApiService.InsertarDevolucionAsync(devolucionDto);
                    if (!apiResult)
                    {
                        throw new Exception("Error al insertar devolución en SAP.");
                    }
                    existingRequest.Estado = "Procesado";
                    _context.ReturnRequests.Update(existingRequest);
                    await _context.SaveChangesAsync();
                }

                return result > 0;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Error al actualizar ReturnRequest (DB): {dbEx.InnerException?.Message ?? dbEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar ReturnRequest: {ex}");
                throw;
            }
        }

        public async Task<List<ReturnRequestEntity>> GetAllReturnRequestsAsync()
        {
            var returnRequests = await _context.ReturnRequests.ToListAsync();

            return returnRequests.Select(rr => new ReturnRequestEntity
            {
                CardCode = rr.CardCode,
                DocDate = rr.DocDate,
                DocDueDate = rr.DocDueDate,
                DocumentLines = rr.DocumentLines.Select(dl => new DocumentLineEntity
                {
                    ItemCode = dl.ItemCode,
                    Quantity = dl.Quantity,
                    WarehouseCode = dl.WarehouseCode
                }).ToList(), // Mapear a DocumentLine2Dto// Convertir de JSON a DTOs
                Estado = rr.Estado,
                FechaInsercion = rr.FechaInsercion,
                CreatedBy = rr.CreatedBy,
                UpdatedBy = rr.UpdatedBy,
                CreatedAt = rr.CreatedAt,
                UpdatedAt = rr.UpdatedAt
            }).ToList();
        }

        public async Task<List<ReturnRequestEntity>> GetReturnRequestsByCardCodeAsync(string cardCode)
        {
            var returnRequests = await _context.ReturnRequests
                .Where(rr => rr.CardCode == cardCode)
                .ToListAsync();

            return returnRequests.Select(rr => new ReturnRequestEntity
            {
                CardCode = rr.CardCode,
                DocDate = rr.DocDate,
                DocDueDate = rr.DocDueDate,
                DocumentLines = rr.DocumentLines.Select(dl => new DocumentLineEntity
                {
                    ItemCode = dl.ItemCode,
                    Quantity = dl.Quantity,
                    WarehouseCode = dl.WarehouseCode
                }).ToList(), // Mapear a DocumentLine2Dto
                Estado = rr.Estado,
                FechaInsercion = rr.FechaInsercion,
                CreatedBy = rr.CreatedBy,
                UpdatedBy = rr.UpdatedBy,
                CreatedAt = rr.CreatedAt,
                UpdatedAt = rr.UpdatedAt
            }).ToList();
        }
    }
}



