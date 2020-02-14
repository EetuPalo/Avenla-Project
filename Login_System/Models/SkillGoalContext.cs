using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class SkillGoalContext : DbContext
    {
        public SkillGoalContext(DbContextOptions<SkillGoalContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<SkillGoals> SkillGoals { get; set; }
    }
}
