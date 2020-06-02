using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class SkillCreateVM
    {
        public int Id { get; set; }
        public string Skill { get; set; }

        public string Description { get; set; }

        public string SkillCategory { get; set; }

        public IList<SelectListItem> SkillCategoryList{ get; set; }

        public SkillCreateVM() { SkillCategoryList = new List<SelectListItem>(); }

    }
}
