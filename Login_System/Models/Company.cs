using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class Company
    {
        public int id { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Company")]
        public string name { get; set; }
        
    }
}
