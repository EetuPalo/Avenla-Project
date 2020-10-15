using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class AppUserVM
    {
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Active { get; set; }

        public string EmpStatus { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public List<string> UserGroups { get; set; }

        public List<UserSkills> UserSkills { get; set; }

        public List<UserGoals> UserGoals { get; set; }

        public List<SkillCourseMember> UserCourses { get; set; }
        
        public List<UserCertificate> UserCertificates { get; set; }

        public List<CompanyGoals> CompanyGoals { get; set; }

    }
}
