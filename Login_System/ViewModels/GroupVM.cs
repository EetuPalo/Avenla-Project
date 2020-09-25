using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Resources;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Login_System.ViewModels
{
    public class GroupVM
    {
        [Display(Name = "MODEL_Name", ResourceType = typeof(Resources.Groups))]
        public string name { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Company", ResourceType = typeof(Resources.Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resources),
        ErrorMessageResourceName = "CompanyRequired")]
        public string company { get; set; }

        public IList<SelectListItem> CompanyList { get; set; }
        public List<SelectListItem> Skills { get; set; }
        public List<Skills> GroupSkills { get; set; }
        public string GroupMembers { get; set; }
        public List<string> ListOfMembers { get; set; }
        public IList<SelectListItem> GroupMembersList { get; set; }

        public int CompanyId { get; set; }

        public GroupVM() 
        { 
            CompanyList = new List<SelectListItem>();
            Skills = new List<SelectListItem>();
            GroupMembersList = new List<SelectListItem>();
            ListOfMembers = new List<string>();
        }

        

        public string Skill { get; set; }
    }
}
