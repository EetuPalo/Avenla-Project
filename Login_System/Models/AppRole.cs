using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class AppRole : IdentityRole<int>
    {
        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
