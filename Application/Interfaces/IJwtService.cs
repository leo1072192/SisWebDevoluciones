﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
namespace Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(string cardCode, string role);
        ClaimsPrincipal GetPrincipalFromToken(string token);
    }
}

