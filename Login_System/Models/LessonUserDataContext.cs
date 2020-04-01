using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class LessonUserDataContext : DbContext
    {
        public LessonUserDataContext(DbContextOptions<LessonUserDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<LessonUser> LessonUsers { get; set; }
    }
}
