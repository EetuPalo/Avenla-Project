using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class GroupMembersDataContext : DbContext
    {
        public GroupMembersDataContext(DbContextOptions<GroupMembersDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<GroupMember> GroupMembers { get; set; }
    }
}
