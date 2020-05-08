using Login_System.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class EditUserVM
    {
        public AppUser User { get; set; }       

        public List<Group> Groups { get; set; }
    }
}
