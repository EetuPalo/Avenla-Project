using Login_System.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Login_System.Models
{
    public class Group
    {
        public int id { get; set; }

        [Display(Name = "MODEL_Name", ResourceType = typeof(Resources.Groups))]
        public string name { get; set; }

        public string company { get; set; }

        public int CompanyId { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
        
    }
}
