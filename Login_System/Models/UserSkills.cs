using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class UserSkills
    {
        public int Id { get; set; }

        public int UserID { get; set; }

        public string SkillName { get; set; }

        public int SkillLevel { get; set; }

        public DateTime Date { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Skill { get; set; }

    }
}
