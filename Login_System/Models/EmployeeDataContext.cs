using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class EmployeeDataContext : DbContext
    {
        public EmployeeDataContext(DbContextOptions<EmployeeDataContext> options) : base(options)
        {
            //This is needed to create the database automatically
            Database.EnsureCreated();
        }
    }
}
