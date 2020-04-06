using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class CreateLessonVM
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string LessonName { get; set; }
        public DateTime Date { get; set; }

        public string Location { get; set; }
        public string DateString { get; set; }

        public string HourString { get; set; }
        public string MinuteString { get; set; }
    }
}
