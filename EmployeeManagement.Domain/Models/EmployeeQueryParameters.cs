using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagement.Domain.Models
{
    public class EmployeeQueryParameters
    {
        public string? Name { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? Department { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "DESC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
