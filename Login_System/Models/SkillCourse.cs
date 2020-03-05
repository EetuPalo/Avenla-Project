using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class SkillCourse
    {
        public int id { get; set; }
        public string CourseName { get; set; }
        public string CourseContents { get; set; }
        public string Location { get; set; } //If it's online, it will be just a link
	public int Length {get; set;} //course length in days
        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
