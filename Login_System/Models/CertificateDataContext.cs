using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class CertificateDataContext : DbContext
    {
        public CertificateDataContext(DbContextOptions<CertificateDataContext> options) : base(options)
        {

        }
        public DbSet<Certificate> Certificates { get; set; }
    }
}
