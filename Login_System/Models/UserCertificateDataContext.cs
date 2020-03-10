using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class UserCertificateDataContext : DbContext
    {
        public UserCertificateDataContext(DbContextOptions<UserCertificateDataContext> options) : base(options)
        {
            
        }
        public DbSet<UserCertificate> UserCertificates { get; set; }
    }
}
