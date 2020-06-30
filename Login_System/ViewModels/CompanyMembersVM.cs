using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class CompanyMembersVM
    {
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public string CompanyName { get; set; }
        public string UserName { get; set; }

    }
}
