using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class SkillCourseDataContext: DbContext
    {
        public SkillCourseDataContext(DbContextOptions<SkillCourseDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<SkillCourse> Courses { get; set; }
    }
}
