using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Login_System.Models
{
    public class SkillCategories
    {
        public int id { get; set; }

        [Display(Name = "MODEL_Name", ResourceType = typeof(Resources.SkillCategories))]
        public string Name { get; set; }

        public string Description { get; set; }

    }
}
