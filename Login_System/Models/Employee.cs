using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class Employee
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Telephone { get; set; }

        public string Email { get; set; }
        public bool Active { get; set; }
    }
}
