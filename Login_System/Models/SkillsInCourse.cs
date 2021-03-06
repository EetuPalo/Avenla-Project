﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class SkillsInCourse
{
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int SkillId { get; set; }
        public int SkillGoal { get; set; }
        public int SkillStartingLevel { get; set; }
    }
}
