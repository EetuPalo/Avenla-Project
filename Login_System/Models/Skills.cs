using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class Skills
    {
        public int Id { get; set; }

        [Display(Name = "MODEL_Skill", ResourceType = typeof(Resources.Skills))]
        public string Skill { get; set; }

        public string Description { get; set; }

        [NotMapped]
        public string OldName { get; set; }
       /* [NotMapped]
        public List<SkillCategories> ListOfCategories { get; set; }
        [NotMapped]
        public IList<SelectListItem> SkillCategoryList { get; set; }

        public Skills() { SkillCategoryList = new List<SelectListItem>(); }
        [NotMapped]
        public SkillCategories SkillCategory {get; set;}
        /*
        [NotMapped]
        public int EntryCount { get; set; }

        [NotMapped]
        public string LatestEntry { get; set; }

        [NotMapped]
        public int LatestEval { get; set; }
        */
    }
}
