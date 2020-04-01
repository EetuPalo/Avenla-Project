using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class LessonDataContext : DbContext
    {
        public LessonDataContext(DbContextOptions<LessonDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Lesson> Lessons { get; set; }
    }
}
