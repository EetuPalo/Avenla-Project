using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class CompanyMember
    {
        public int CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public AppUser AppUser  { get; set; }
    }
}
