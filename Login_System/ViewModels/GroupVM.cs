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
        public int CompanyId { get; set; }

        public GroupVM() { CompanyList = new List<SelectListItem>(); }
    }
}
