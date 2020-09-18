using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class UserGoalsVM
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public int SkillId { get; set; }

        public string SkillName { get; set; }

        public int SkillGoal { get; set; }

        public IList<SelectListItem> AvailableSkillList { get; set; }

        public UserGoalsVM()
        {
            AvailableSkillList = new List<SelectListItem>();

        }
    }
}
