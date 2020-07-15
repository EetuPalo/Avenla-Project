using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class GroupMember
    {
        public int Id { get; set; }

        [Display(Name = "MODEL_UserID", ResourceType = typeof(Resources.Groups))]
        public int UserID { get; set; }

        [Display(Name = "MODEL_GroupID", ResourceType = typeof(Resources.Groups))]
        public int GroupID { get; set; }

        [Display(Name = "MODEL_UserName", ResourceType = typeof(Resources.Groups))]
        public string UserName { get; set; }

        [Display(Name = "MODEL_GroupName", ResourceType = typeof(Resources.Groups))]
        public string GroupName { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Group { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Uname { get; set; }

        [NotMapped]
        public List<AppUser> Members { get; set; }
    }
}
