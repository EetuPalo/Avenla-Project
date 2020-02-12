using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class GroupMember
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Group { get; set; }
    }
}
