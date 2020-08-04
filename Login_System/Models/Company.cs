using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Login_System.Models
{
    public class Company
    {
        public int Id { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Company")]
        public string Name { get; set; }

        [AllowHtml]
        public string Description { get; set; }
        
    }
}
