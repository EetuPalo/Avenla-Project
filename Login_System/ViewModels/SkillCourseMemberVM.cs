﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class SkillCourseMemberVM
    {
        public int Id { get; set; }

        public int UserID { get; set; }

        public string UserName { get; set; }

        public string CourseName { get; set; }

        public int CourseID { get; set; }
        public DateTime CompletionDate { get; set; }
        public int DaysCompleted { get; set; }
        public int CourseLength { get; set; }
        public string Status { get; set; }
        public string CourseGrade { get; set; }
    }
}
