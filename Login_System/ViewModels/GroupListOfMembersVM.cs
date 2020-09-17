// RRS
using Microsoft.AspNetCore.Mvc.Rendering;
using Login_System.Models;
// END RRS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class GroupListOfMembersVM
    {
        public string GroupMembers { get; set; }
        public List<string> ListOfMembers { get; set; }
        public IList<SelectListItem> GroupMembersList { get; set; }
        public GroupListOfMembersVM() { 
            GroupMembersList = new List<SelectListItem>();
            ListOfMembers = new List<string>();
        }


    }
}
