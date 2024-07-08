using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IReturnRequestService
    {
        Task<bool> InsertarReturnRequestAsync(ReturnRequestDto returnRequest);
        Task<bool> ActualizarReturnRequestAsync(int id, ReturnRequestDto returnRequest); // Nuevo método para actualizar
    }
}
