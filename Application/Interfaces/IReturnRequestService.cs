using System.Threading.Tasks;
using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IReturnRequestService
    {
        Task<bool> InsertarReturnRequestAsync(ReturnRequestDto returnRequest);
        Task<bool> ActualizarReturnRequestAsync(int id, ReturnRequestDto returnRequest); // Nuevo método para actualizar
        Task<List<ReturnRequestEntity>> GetAllReturnRequestsAsync();
        Task<List<ReturnRequestEntity>> GetReturnRequestsByCardCodeAsync(string cardCode);

    }
}
