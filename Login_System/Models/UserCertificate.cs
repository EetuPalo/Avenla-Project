using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class UserCertificate
    {
        public int Id { get; set; }

        public int UserID { get; set; }

        public int CertificateID { get; set; }

        public string UserName { get; set; }

        public string CertificateName { get; set; }

        public string Organization { get; set; }

        public DateTime GrantDate { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> CertificateList { get; set; }

        /*
        public UserCertificate()
        {
            CertificateList = new List<SelectListItem>();
        }
        */
    }
}
