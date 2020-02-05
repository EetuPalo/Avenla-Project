using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class SkillDataContext : DbContext
    {
        public SkillDataContext(DbContextOptions<SkillDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Skills> Skills { get; set; }
    }
}
