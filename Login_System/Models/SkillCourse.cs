using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Login_System.Models
{
    public class SkillCourse
    {
        public int id { get; set; }

        [Display(Name = "MODEL_CourseName", ResourceType = typeof(Resources.Courses))]
        public string CourseName { get; set; }

        [AllowHtml]
        [Display(Name = "MODEL_CourseContents", ResourceType = typeof(Resources.Courses))]
        public string CourseContents { get; set; }

        [Display(Name = "MODEL_LOCATION", ResourceType = typeof(Resources.Courses))]
        public string Location { get; set; } //If it's online, it will be just a link

        [Display(Name = "MODEL_LENGTH", ResourceType = typeof(Resources.Courses))]
        public int Length {get; set;} //course length in days
        [NotMapped]
        public bool IsSelected { get; set; }
        [NotMapped]
        public bool MemberStatus { get; set; }
        [NotMapped]
        public bool CompleteStatus { get; set; }
    }
}
