using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByCardCodeAsync(string cardCode);
        Task AddUserAsync(User user);
    }
}
