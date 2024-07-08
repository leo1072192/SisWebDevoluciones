using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByCardCodeAsync(string cardCode);
        Task<bool> VerifyPasswordAsync(User user, string password);
        Task CreateUserAsync(string cardCode, string password);
    }
}