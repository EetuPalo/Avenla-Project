using Login_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class UserSkillsVM
    {
        public int Id { get; set; }

        public int UserID { get; set; }

        public string UserName { get; set; }

        public string SkillName { get; set; }

        public int SkillLevel { get; set; }

        public DateTime Date { get; set; }
    }
}
