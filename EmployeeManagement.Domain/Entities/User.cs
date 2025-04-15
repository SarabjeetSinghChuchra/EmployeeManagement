using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagement.Domain.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]    
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //public ICollection<Employee> CreatedEmployess { get; set; }
        //public ICollection<Employee> UpdatedEmployess { get; set; }

    }
}
