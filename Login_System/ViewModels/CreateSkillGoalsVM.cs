using Login_System.Models;
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
        public int SkillCounter { get; set; }

        public List<SkillGoals> SkillGoals { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Skills { get; set; }

        /*
        public Dictionary<int, SkillGoals> SkillGoals { get; set; }

        public int SkillCounter { get; set; }
        */
    }
}
