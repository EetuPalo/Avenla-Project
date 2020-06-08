using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Login_System.ViewModels
{
    public class DashboardVM
    {
        public List<UserSkills> UserSkills { get; set; }
        public List<UserCertificate> UserCertificates { get; set; }
        public List<SkillCourseMember> UserCourses { get; set; }
        public List<SkillGoals> UserGoals { get; set; }
        public List<GroupMember> UserGroups { get; set; }
        public List<Lesson> Lessons { get; set; }
        public IList<SelectListItem> GroupsDD { get; set; }
        public DashboardVM()
        { GroupsDD = new List<SelectListItem>(); }
    }
}
