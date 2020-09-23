using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Login_System.Models
{
    public class CompanyGroups
    {
        [Key]
        public int CompanyGroupId { get; set; }

        public string CompanyGroupName { get; set; }

        [NotMapped]
        public List<string> Company { get; set; }
        [NotMapped]
        public List<string> Skill { get; set; }
        [NotMapped]
        public IList<SelectListItem> CompanyList { get; set; }
        [NotMapped]
        public IList<SelectListItem> SkillList { get; set; }


        public CompanyGroups()
        {
            CompanyList = new List<SelectListItem>();
            SkillList = new List<SelectListItem>();

        }
        [NotMapped]
        public List<Company> companiesInGroups { get; set; }

  
    }
}
