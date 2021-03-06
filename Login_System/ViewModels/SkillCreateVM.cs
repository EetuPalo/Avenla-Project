﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Login_System.Models;

namespace Login_System.ViewModels
{
    public class SkillCreateVM
    {
        public int Id { get; set; }
        public string Skill { get; set; }
        public Skills skill { get; set; }
        public string Description { get; set; }

        public string SkillCategory { get; set; }

        [NotMapped]
        public string OldName { get; set; }

        public List<SkillCategories> skillCategories { get; set; }
        
        public List<SkillCategories> ListOfCategories { get; set; }
        
        public IList<SelectListItem> SkillCategoryList{ get; set; }

        public SkillCreateVM() { SkillCategoryList = new List<SelectListItem>(); }

    }
}
