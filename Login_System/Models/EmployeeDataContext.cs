using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;

namespace Login_System.Models
{
    public class EmployeeDataContext : DbContext
    {
        public EmployeeDataContext(DbContextOptions<EmployeeDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Login_System.Models.Employee> Employee { get; set; }
    }
}
