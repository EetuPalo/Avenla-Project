using Login_System.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class CreateCompanyGoals
    {
        public int Id { get; set; }
        public int CompanyGoal { get; set; }
        public int CompanyID { get; set; }
        public List<int> SkillID { get; set; }

        public List<Skills> Skills { get; set; }

    }

}
