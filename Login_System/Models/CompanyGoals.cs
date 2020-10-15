using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class CompanyGoals
    {
        public int Id { get; set; }

        public int CompanyGoal { get; set; }
        public int CompanyID { get; set; }
        public int SkillID { get; set; }
    }
}
