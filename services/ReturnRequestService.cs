using System;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Infrastructure.Services
{
    public class ReturnRequestService : IReturnRequestService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AppDbContext _context;
        private readonly IDevolucionesApiService _devolucionesApiService;
        private readonly EmailService _emailService;
        public ReturnRequestService(EmailService emailService,AppDbContext context, IDevolucionesApiService devolucionesApiService, IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _devolucionesApiService = devolucionesApiService ?? throw new ArgumentNullException(nameof(devolucionesApiService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<bool> InsertarReturnRequestAsync(ReturnRequestDto returnRequest)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(); // Iniciar una transacción
            try
            {
                // Convertir DocumentLines de DTO a Entity
                var documentLines = returnRequest.DocumentLines.Select(dl => new DocumentLineEntity
                {
                    Id=dl.Id,
                    OrderId=dl.OrderId,
                    ItemCode = dl.ItemCode,
                    Quantity = dl.Quantity,
                    WarehouseCode = dl.WarehouseCode,
                    devolucionQuantity = dl.devolucionQuantity,
                }).ToList();

                // Crear la entidad ReturnRequest
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
                };

                // Insertar el nuevo ReturnRequest en la base de datos
                _context.ReturnRequests.Add(newReturnRequest);
                await _context.SaveChangesAsync();
                var returnRequestId = newReturnRequest.Id;
                // Actualizar las líneas de documento de la orden correspondiente
                foreach (var dl in returnRequest.DocumentLines)
                {
                    var order = await _context.Orders
                        .Include(o => o.DocumentLines)
                        .FirstOrDefaultAsync(o => o.Id == dl.OrderId);

                    if (order != null)
                    {
                        var documentLine = order.DocumentLines.FirstOrDefault(l => l.Id == dl.Id);
                        if (documentLine != null)
                        {
                            documentLine.Quantity -= dl.devolucionQuantity; // Reducir la cantidad de la línea de documento
                            await _orderRepository.UpdateOrderAsync(order); // Actualizar la orden
                        }
                    }
                }

                await transaction.CommitAsync(); // Confirmar la transacción
                                                 // Enviar correo electrónico
                var user = await _context.Users.SingleOrDefaultAsync(u => u.CardCode == returnRequest.CardCode);
                Console.WriteLine(user.Email);
                if (user != null)
                {
                    var subject = "Solicitud Devolucion";
                    var body = $"Su solicitud de devolucion con numero de referencia #: {returnRequestId} ha sido recibido.";
                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync(); // Revertir la transacción en caso de error
                Console.WriteLine($"Error al insertar ReturnRequest (DB): {dbEx.InnerException?.Message ?? dbEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Revertir la transacción en caso de error
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
                {   Id=dl.Id,
                    OrderId=dl.OrderId,
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

                if (existingRequest.Estado == "APROBADO")
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
                  
                    var apiResult = await _devolucionesApiService.InsertarDevolucionAsync(devolucionDto);
                    if (!apiResult)
                    {
                        throw new Exception("Error al insertar devolución en SAP.");
                    }
                    else
                    {
                        existingRequest.Estado = "Aprobado";
                        _context.ReturnRequests.Update(existingRequest);
                        await _context.SaveChangesAsync();
                        // Enviar correo electrónico
                        var user = await _context.Users.SingleOrDefaultAsync(u => u.CardCode == returnRequest.CardCode);
                        Console.WriteLine(user.Email);
                        var subject = "Solicitud Devolucion";
                        var body = $"Su solicitud de devolucion con numero de referencia #: {id} ha sido APROBADA.";
                        await _emailService.SendEmailAsync(user.Email, subject, body);
                    }
                }

                if (existingRequest.Estado == "RECHAZADO")
                {
                   
                    // Actualizar las líneas de documento de la orden correspondiente
                    foreach (var dl in returnRequest.DocumentLines)
                    {
                        var order = await _context.Orders
                            .Include(o => o.DocumentLines)
                            .FirstOrDefaultAsync(o => o.Id == dl.OrderId);

                        if (order != null)
                        {
                            var documentLine = order.DocumentLines.FirstOrDefault(l => l.Id == dl.Id);
                            if (documentLine != null)
                            {
                                documentLine.Quantity += dl.devolucionQuantity; // Reducir la cantidad de la línea de documento
                                await _orderRepository.UpdateOrderAsync(order); // Actualizar la orden
                            }
                        }
                    }

                    existingRequest.Estado = "Rechazado";
                        _context.ReturnRequests.Update(existingRequest);
                        await _context.SaveChangesAsync();
                        // Enviar correo electrónico
                        var user = await _context.Users.SingleOrDefaultAsync(u => u.CardCode == returnRequest.CardCode);
                        Console.WriteLine(user.Email);
                        var subject = "Solicitud Devolucion";
                        var body = $"Su solicitud de devolucion con numero de referencia #: {id} ha sido Rechazado.";
                        await _emailService.SendEmailAsync(user.Email, subject, body);
                    
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
                Id=rr.Id,
                CardCode = rr.CardCode,
                DocDate = rr.DocDate,
                DocDueDate = rr.DocDueDate,
                DocumentLines = rr.DocumentLines.Select(dl => new DocumentLineEntity
                {
                    Id=dl.Id,
                    OrderId=dl.OrderId,
                    ItemCode = dl.ItemCode,
                    Quantity = dl.Quantity,
                    devolucionQuantity = dl.devolucionQuantity,
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
                Id=rr.Id,
                CardCode = rr.CardCode,
                DocDate = rr.DocDate,
                DocDueDate = rr.DocDueDate,
                DocumentLines = rr.DocumentLines.Select(dl => new DocumentLineEntity
                {
                    Id = dl.Id,
                    OrderId = dl.OrderId,
                    ItemCode = dl.ItemCode,
                    Quantity = dl.Quantity,
                    WarehouseCode = dl.WarehouseCode,
                    devolucionQuantity = dl.devolucionQuantity,

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



