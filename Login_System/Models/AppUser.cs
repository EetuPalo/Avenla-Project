using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class AppUser : IdentityUser<int>
    {

        [Display(Name = "FirstName", ResourceType = typeof(Resources.Resources))]
        public string FirstName { get; set; }

        [Display(Name = "LastName", ResourceType = typeof(Resources.Resources))]
        public string LastName { get; set; }

        public string Active { get; set; }

        public string EmpStatus { get; set; }
        

        [NotMapped]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [NotMapped]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
        [NotMapped]
        public string TempUserName { get; set; }
        
    }
}
