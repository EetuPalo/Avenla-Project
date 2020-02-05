using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class Skills
    {
        public int Id { get; set; }

        public string Skill { get; set; }

        public int SkillLevel { get; set; }

        public int UserID { get; set; }
    }
}
