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
