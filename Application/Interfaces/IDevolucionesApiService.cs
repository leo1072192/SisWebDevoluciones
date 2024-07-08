// IDevolucionesApiService.cs
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IDevolucionesApiService
    {
        Task<bool> InsertarDevolucionAsync(DevolucionDto devolucion);
    }
}
