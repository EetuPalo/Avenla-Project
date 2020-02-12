using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class UserSkillsWithSkillVM
    {
         //public UserSkills UserSkill { get; set; }

         public Dictionary<int, string> SkillList { get; set; }

         //public string SkillName { get; set; }

         public int SkillCount { get; set; }

         public Dictionary<int, int> SkillLevel { get; set; }
    }
}
