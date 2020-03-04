using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class GroupUser
    {
        public string UserId { get; set; }
        public int GroupId { get; set; }
        public string UserName { get; set; }
        public string GroupName { get; set; }
        public bool IsSelected { get; set; }
    }
}
