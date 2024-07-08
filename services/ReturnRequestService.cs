using System;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

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
                    WarehouseCode = dl.WarehouseCode
                }).ToList();

                var newReturnRequest = new ReturnRequestEntity
                {
                    CardCode = returnRequest.CardCode,
                    DocDate = DateTime.SpecifyKind(returnRequest.DocDate, DateTimeKind.Utc),
                    DocDueDate = DateTime.SpecifyKind(returnRequest.DocDueDate, DateTimeKind.Utc),
                    DocumentLines = documentLines,
                    Estado = returnRequest.Estado ?? "Pendiente",
                    FechaInsercion = DateTime.SpecifyKind(returnRequest.FechaInsercion, DateTimeKind.Utc),
                    CreatedBy = returnRequest.CreatedBy,
                    UpdatedBy = returnRequest.UpdatedBy,
                    CreatedAt = DateTime.SpecifyKind(returnRequest.CreatedAt, DateTimeKind.Utc),
                    UpdatedAt = DateTime.SpecifyKind(returnRequest.UpdatedAt, DateTimeKind.Utc),
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
                existingRequest.DocDate = DateTime.SpecifyKind(returnRequest.DocDate, DateTimeKind.Utc);
                existingRequest.DocDueDate = DateTime.SpecifyKind(returnRequest.DocDueDate, DateTimeKind.Utc);
                existingRequest.DocumentLines = returnRequest.DocumentLines.Select(dl => new DocumentLineEntity
                {
                    ItemCode = dl.ItemCode,
                    Quantity = dl.Quantity,
                    WarehouseCode = dl.WarehouseCode
                }).ToList();
                existingRequest.Estado = returnRequest.Estado ?? existingRequest.Estado;
                existingRequest.FechaInsercion = DateTime.SpecifyKind(returnRequest.FechaInsercion, DateTimeKind.Utc);
                existingRequest.CreatedBy = returnRequest.CreatedBy;
                existingRequest.UpdatedBy = returnRequest.UpdatedBy;
                existingRequest.CreatedAt = DateTime.SpecifyKind(returnRequest.CreatedAt, DateTimeKind.Utc);
                existingRequest.UpdatedAt = DateTime.SpecifyKind(returnRequest.UpdatedAt, DateTimeKind.Utc);

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
                            Quantity = dl.Quantity,
                            WarehouseCode = dl.WarehouseCode
                        }).ToList()
                    };

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
    }
}
