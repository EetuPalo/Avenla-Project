﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.ViewModels
{
    public class GroupMemberVM
    {
        public int Id { get; set; }

        public int UserID { get; set; }

        public string UserName { get; set; }    
        
        public string GroupName { get; set; }
    }
}
