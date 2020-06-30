using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class CompanyMembersVM
    {
        public int Id { get; set; }
        [NotMapped]
        public string name { get; set; }
        [NotMapped]
        public string Description { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public string CompanyName { get; set; }
        public string UserName { get; set; }
        [NotMapped]
        public IList<SelectListItem> userList { get; set; }
    
        public CompanyMembersVM() 
        { 
            userList = new List<SelectListItem>();
        }

    }
}
