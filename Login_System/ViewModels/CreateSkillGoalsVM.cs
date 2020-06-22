using Login_System.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class CreateSkillGoalsVM
    {
        public List<Skills> GroupSkills { get; set; }
        public int Skillid { get; set; }
        public List<int> SkillGoal { get; set; }
        public List<SkillGoals> SkillGoals { get; set; }

        public string Skill { get; set; }
        public int Level { get; set; }

        public int Groupid { get; set; }

        public string GroupName { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Skills { get; set; }

        /*
        public Dictionary<int, SkillGoals> SkillGoals { get; set; }

        public int SkillCounter { get; set; }
        */
    }
}
