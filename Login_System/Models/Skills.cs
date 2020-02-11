using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class Skills
    {
        public int Id { get; set; }

        public string Skill { get; set; }

        [NotMapped]
        public int EntryCount { get; set; }

        [NotMapped]
        public string LatestEntry { get; set; }

        [NotMapped]
        public int LatestEval { get; set; }
    }
}
