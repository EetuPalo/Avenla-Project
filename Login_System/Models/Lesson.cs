using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string LessonName { get; set; }
        public DateTime Date { get; set; }

        public string Location { get; set; }

        [NotMapped]
        public string DateString { get; set; }
        [NotMapped]
        public bool LessonStatus { get; set; }
        [NotMapped]
        public List <Lesson> LessonList { get; set; }
    }
}
