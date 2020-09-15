using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Login_System.Models
{
    public class CompanyGroups
    {

        public int CompanyGroupId { get; set; }

        //[DataType(DataType.Text)]
        //[DisplayName("Company Group")]
        public string CompanyGroupName { get; set; }

    }
}
