using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISapB1Service
    {
        Task<SapUserDto> VerifyAndAuthenticateUserAsync(string cardCode, string password);
    }

}