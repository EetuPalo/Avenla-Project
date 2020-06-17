using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class SkillGoals
    {
        public int Id { get; set; }

        [Display(Name = "MODEL_GroupName", ResourceType = typeof(Resources.SkillGoals))]
        public string GroupName { get; set; }

        [Display(Name = "MODEL_SkillName", ResourceType = typeof(Resources.SkillGoals))]
        public string SkillName { get; set; }

        [Display(Name = "MODEL_SkillGoal", ResourceType = typeof(Resources.SkillGoals))]
        public int SkillGoal { get; set; }

        [Display(Name = "MODEL_Date", ResourceType = typeof(Resources.SkillGoals))]
        public DateTime Date { get; set; }
        public int Skillid { get; set; }

        [NotMapped]
        public IList<SelectListItem> Skills { get; set; }

        [NotMapped]
        public int LatestGoal { get; set; }

        public SkillGoals()
        {
            Skills = new List<SelectListItem>();
        }
    }
}
