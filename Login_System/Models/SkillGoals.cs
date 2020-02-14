using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class SkillGoals
    {
        public int Id { get; set; }

        public string GroupName { get; set; }

        public string SkillName { get; set; }

        public int SkillGoal { get; set; }

        [NotMapped]
        public IList<SelectListItem> Skills { get; set; }

        public SkillGoals()
        {
            Skills = new List<SelectListItem>();
        }
    }
}
