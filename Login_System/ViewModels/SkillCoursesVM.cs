using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Login_System.ViewModels
{
    public class SkillCoursesVM
    {
        public int id { get; set; }
        public string CourseName { get; set; }

        [AllowHtml]
        public string CourseContents { get; set; }
        public string Location { get; set; } //If it's online, it will be just a link
        public int Length { get; set; } //course length in days
        public bool MemberStatus { get; set; }
    }
}
