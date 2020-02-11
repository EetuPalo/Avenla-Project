using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class GroupsDataContext : DbContext
    {
        public GroupsDataContext(DbContextOptions<GroupsDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Group> Group { get; set; }
    }
}