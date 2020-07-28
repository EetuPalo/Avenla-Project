using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class UserSkillsWithSkillVM
    {
         
         public List<Skills> SkillList { get; set; }
         
         public List<UserSkills> UserSkill { get; set; }

         //public string SkillName { get; set; }

         public int SkillCount { get; set; }

        //testing with different way of life 
         public List<int> SkillLevel { get; set; }
         public List<int> Skillid { get; set; }
         public int Skilllevel { get; set; }




    }
}
