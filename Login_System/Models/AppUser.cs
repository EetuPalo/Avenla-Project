using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Active { get; set; }

        public string EmpStatus { get; set; }
        

        [NotMapped]
        public string NewPassword { get; set; }
        [NotMapped]
        public string ConfirmNewPassword { get; set; }
        
    }
}
