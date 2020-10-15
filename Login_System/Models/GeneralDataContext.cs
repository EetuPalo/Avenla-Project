using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;
using Login_System.Controllers;
using Login_System.ViewModels;

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
        public DbSet<UserGoals> UserGoals { get; set; }
        public DbSet<SkillGoals> SkillGoals { get; set; }
        public DbSet<SkillCategories> SkillCategories { get; set; }

        public DbSet<SkillsInCategory> SkillsInCategory { get; set; }
        //IDENTITY
        public DbSet<AppUser> AppUser { get; set; }
        //IDENTITY
        public DbSet<Company> Company { get; set; }
        public DbSet<CompanyGroups> CompanyGroups { get; set; }

        public DbSet<CompanyMember> CompanyMembers {get; set;}
        public DbSet<SkillsInCourse> SkillsInCourse { get; set; }
        public DbSet<CompanyGroupSkill> CompanyGroupSkills { get; set; }
        public DbSet<CompanyGroupCertificate> CompanyGroupCertificates { get; set; }
        public DbSet<CompanyGroupMember> CompanyGroupMembers{ get; set; }
        public DbSet<CompanyGoals> CompanyGoals { get; set; }

        protected override void OnModelCreating(ModelBuilder mb) { 
        

            mb.Entity<SkillsInCategory>()
                .HasKey(x => new { x.SkillId, x.CategoryId});


            mb.Entity<CompanyMember>()
                .HasKey(x => new { x.CompanyId, x.UserId});

        }

    }
}
