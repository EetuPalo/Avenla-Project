using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class DeleteRole
    {
        public DeleteRole()
        {
            Users = new List<string>();
        }

        public string Id { get; set; }

        [Display(Name = "MODEL_DeleteRole_RoleName", ResourceType = typeof(Resources.Admin))]
        public string RoleName { get; set; }


        public List<string> Users { get; set; }
    }
}
