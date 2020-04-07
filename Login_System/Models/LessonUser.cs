using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class LessonUser
    {
        public int Id { get; set; }
        public int LessonID { get; set; }
        public int MemberID { get; set; }
        public string MemberName { get; set; }
        public bool Attending { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
