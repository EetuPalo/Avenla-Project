using Login_System.Models;
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
        public List<int> SelectedUserIds { get; set; }
        public string name { get; set; }
   
        public string Description { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public string CompanyName { get; set; }
        public string UserName { get; set; }

        public List<AppUser> Users { get; set; } 
      
        public IList<SelectListItem> userList { get; set; }
    
        public CompanyMembersVM() 
        { 
            userList = new List<SelectListItem>();
        }

    }
}
