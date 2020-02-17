using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class CreateSkillGoalsVM
    {
        public Dictionary<int, SkillGoals> SkillGoals { get; set; }

        //public Dictionary<int, int> SkillLevel { get; set; }

        public int SkillCounter { get; set; }
    }
}
