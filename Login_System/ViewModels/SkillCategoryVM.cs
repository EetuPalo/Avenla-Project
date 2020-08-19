using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class SkillCategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public List<int> Skills { get;set; }

        public List<string> currentSkills { get; set; }

        public List<Skills> SkillsInCategory {get; set;}

        public IList<SelectListItem> SkillList { get; set; }

        public SkillCategoryVM()
        {
            SkillList = new List<SelectListItem>();
           
        }
    }
}
