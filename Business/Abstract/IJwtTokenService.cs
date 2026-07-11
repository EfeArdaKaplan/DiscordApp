using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IJwtTokenService
    {
        string GenerateToken(AppUser user, IList<string> roles);
    }
}
