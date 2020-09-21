using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class CompanyGroupSkill
{

        public int Id { get; set; }

        public int SkillId { get; set; }
        public int CompanyGroupId { get; set; }
        public int CompanyId { get; set; }
    }
}
