using Login_System.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class SkillGoalIndexVM
    {
        public List<SkillGoals> Goals { get; set; }

        [NotMapped]
        public IList<SelectListItem> SkillDates { get; set; }

        public string SelectedDate { get; set; }

        public string GroupName { get; set; }

        public SkillGoalIndexVM()
        {
            SkillDates = new List<SelectListItem>();
        }
    }
}
