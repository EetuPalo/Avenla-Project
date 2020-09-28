using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Login_System.ViewModels
{
    public class AdvancedSearchVM
    {
        public string Company { get; set; }
        public bool CompanyGroupSearch { get; set; }

        public string Skill { get; set; }

        public int SkillLevel { get; set; }

        public string Groups { get; set; }

        public string Certificate { get; set; }

        //public List<AppUser> Users { get; set; }
        public List<int> adminCompanyIds { get; set; }
        public List<(AppUser, List<int>, string)> Users{get; set;}

        public IList<SelectListItem> CertificateList { get; set; }

        public IList<SelectListItem> GroupList { get; set; }
        public IList<SelectListItem> SkillList { get; set; }
        public IList<SelectListItem> CompanyList { get; set; }
        public AdvancedSearchVM()
        {
            CertificateList = new List<SelectListItem>();
            GroupList = new List<SelectListItem>();
            SkillList = new List<SelectListItem>();
            CompanyList = new List<SelectListItem>();
        }
    }

}
