using System;
using System.Collections.Generic;
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

        public DateTime GrantDate { get; set; }
    }
}
