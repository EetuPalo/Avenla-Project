﻿using Login_System.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class CreateSkillGoalsVM
    {
        public List<Skills> GroupSkills { get; set; }
        [DataType(DataType.Text)]
        public Skills skill { get; set; }
        public int Skillid { get; set; }
        public int SkillGoal { get; set; }
        public List<SkillGoals> SkillGoals { get; set; }

        [DataType(DataType.Text)]
        
        public int Skillgoal { get; set; }
        public string Skill { get; set; }
        public int Level { get; set; }

        public int Groupid { get; set; }

        public string GroupName { get; set; }
        [NotMapped]
        public List<SelectListItem> Skills { get; set; }

        public CreateSkillGoalsVM()
        {
            Skills = new List<SelectListItem>();
        }
    }
}
