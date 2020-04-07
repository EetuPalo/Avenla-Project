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
        public List<SkillCourse> Courses { get; set; }
        public List<Lesson> Lessons { get; set; }
        public Dictionary<int, string?> Durations { get; set; }
    }
}
