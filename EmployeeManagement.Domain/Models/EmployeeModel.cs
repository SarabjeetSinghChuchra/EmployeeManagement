using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagement.Domain.Entities
{
    public class EmployeeModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(50, ErrorMessage = "Name can't exceed 100 charcters.")]
        public string FullName { get; set; }

        [Range(0,double.MaxValue, ErrorMessage = "Salary must be positive number.")]
        public decimal Salary { get; set; }

        [Required]
        [StringLength(50)]
        public string Department { get; set; }
        public bool IsDeleted { get; set; } = false;
        [Required]
        public string SSN { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
    }
    public class EmployeeModelBulkUpload
    {
        public string FullName { get; set; }
        public string Department { get; set; }
        public decimal Salary { get; set; }
        public bool IsDeleted { get; set; }
        public string EncryptedSSN { get; set; } 
    }

}

