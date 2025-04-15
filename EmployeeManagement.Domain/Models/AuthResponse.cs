using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagement.Domain.Moderls
{
    public class AuthResponse
    {
        public String Token { get; set; }
        public String RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
