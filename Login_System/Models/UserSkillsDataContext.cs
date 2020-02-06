using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class UserSkillsDataContext : DbContext
    {
        public UserSkillsDataContext(DbContextOptions<UserSkillsDataContext> options) : base(options)
        {
            //Database.EnsureCreated();
        }
        public DbSet<UserSkills> UserSkills { get; set; }
    }
}
