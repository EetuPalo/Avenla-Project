using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class CompanyMembersVM
    {
        public int id { get; set; }
        public string name { get; set; }
        public string Description { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public string CompanyName { get; set; }
        public string UserName { get; set; }
        public IList<SelectListItem> userList { get; set; }

        public CompanyMembersVM() 
        { 
            userList = new List<SelectListItem>();
        }

    }
}
