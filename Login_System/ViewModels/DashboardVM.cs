using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Login_System.ViewModels
{
    public class DashboardVM
    {
        public UserSkillsVM UserSkills { get; set; }
        public IList<SelectListItem> CertificateList { get; set; }

        public IList<SelectListItem> GroupList { get; set; }
        public IList<SelectListItem> SkillList { get; set; }

        public IList<SelectListItem> CompanyList { get; set; }

        public DashboardVM()
        {
            CertificateList = new List<SelectListItem>();
            GroupList = new List<SelectListItem>();
            SkillList = new List<SelectListItem>();
            CompanyList = new List<SelectListItem>();
        }
    }
}
