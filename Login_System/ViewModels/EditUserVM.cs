using Login_System.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Login_System.ViewModels
{
    public class EditUserVM
    {
        public AppUser User { get; set; }       

        public List<Group> Groups { get; set; }
        public string Company { get; set; }

        public List<AppRole> Roles { get; set; }
        public IList<SelectListItem> CompanyList { get; set; }

        public EditUserVM() { CompanyList = new List<SelectListItem>(); }
    }
}
