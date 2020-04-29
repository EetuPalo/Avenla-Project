using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class Certificate
    {
        public int Id { get; set; }

        [Display(Name = "MODEL_Name", ResourceType = typeof(Resources.Certificates))]
        public string Name { get; set; }

        [Display(Name = "MODEL_Organization", ResourceType = typeof(Resources.Certificates))]
        public string Organization { get; set; }
    }
}
