using Login_System.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class GrantCertificateVM
{
        public Certificate Certificate { get; set; }

        public List<int> UserIds { get; set; }

        public IList<SelectListItem> UserList { get; set; }

        public GrantCertificateVM()
        {
            UserList = new List<SelectListItem>();
        }
    }
}
