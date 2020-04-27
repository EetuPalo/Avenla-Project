using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class CreateRole
    {
        [Required]
        [Display(Name = "MODEL_CreateRole_RoleName", ResourceType = typeof(Resources.Admin))]
        public string RoleName { get; set; }
    }
}
