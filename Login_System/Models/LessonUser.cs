using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class LessonUser
    {
        public int Id { get; set; }

        [Display(Name = "MODEL_User_LessonID", ResourceType = typeof(Resources.Lessons))]
        public int LessonID { get; set; }

        [Display(Name = "MODEL_User_MemberID", ResourceType = typeof(Resources.Lessons))]
        public int MemberID { get; set; }

        [Display(Name = "MODEL_User_MemberName", ResourceType = typeof(Resources.Lessons))]
        public string MemberName { get; set; }

        [Display(Name = "MODEL_User_Attending", ResourceType = typeof(Resources.Lessons))]
        public bool Attending { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
