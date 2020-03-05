using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class SkillCourseMember
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string CourseName { get; set; }
        public string Status { get; set; } //In-progress, Completed, Dropout, Planning
	public int DaysCompleted { get; set; } //Days in the course user has completed
        public DateTime CompletionDate { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Uname { get; set; }

    }
}
