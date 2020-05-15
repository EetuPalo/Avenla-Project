using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class GeneralDataContext : DbContext
    {
        public GeneralDataContext(DbContextOptions<GeneralDataContext> options) : base(options)
        {

        }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<UserCertificate> UserCertificates { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonUser> LessonUsers { get; set; }
        public DbSet<SkillCourse> Courses { get; set; }
        public virtual DbSet<SkillCourseMember> SkillCourseMembers { get; set; }
        public DbSet<Skills> Skills { get; set; }
        public DbSet<UserSkills> UserSkills { get; set; }
        public DbSet<SkillGoals> SkillGoals { get; set; }
        //IDENTITY
        public DbSet<AppUser> AppUser { get; set; }
    }
}
