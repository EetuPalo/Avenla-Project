using Login_System.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace Login_System.ViewModels
{
    public class SkillCoursesVM
    {
        public List<SkillCourse> Courses { get; set; }
        public List<Lesson> Lessons { get; set; }
        public Dictionary<int, string?> Durations { get; set; }
        [Required]
        public string CourseName { get; set; }

        public string Location { get; set; }

        public int Length { get; set; }

        public string CourseContents { get; set; }
        //public string[] Skill { get; set; }
        public List<string> Skill {get;set;}
        public string[] goal { get; set; }
        public string[] startLevel { get; set; }
        public List<SelectListItem> SkillList { get; set; }
        public int id { get; set; }
        public SkillCourse skillCourse { get; set; }
        public SkillCoursesVM()
        {
            SkillList = new List<SelectListItem>();
        }
    }
}
