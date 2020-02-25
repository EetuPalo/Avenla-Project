using System;
using System.Collections.Generic;
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
        public string Status { get; set; } //In-progress, Completed, Dropout, Planning

    }
}
