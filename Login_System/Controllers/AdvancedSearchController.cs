using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Login_System.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity;

namespace Login_System.Controllers
{
    public class AdvancedSearchController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        public AdvancedSearchController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }
        public async Task<IActionResult> Index(string searchString, string SkillList)
        {
            var model = new AdvancedSearchVM();
            var userList = new List<AppUser>();
            model.Users = userList;

            IQueryable<string> SkillQuery = from s in _context.Skills
                                            orderby s.Skill
                                            select s.Skill;
            var items = from m in _context.Skills
                        select m;
            //TODO:
            //SearchString should be replaced with a dropdown that's populated with all the skills that are in the database
            //Later there should also be an option to select multiple skills for the search
            //Next to add similar functions for:
            //Search by group
            //Filter by skill level (both min and max)
            //Certificates

            //Note that all these different forms need to be available at the same time and selected/deselected as the user wants

            if (!string.IsNullOrEmpty(searchString))
            {
                items = items.Where(s => s.Skill.Contains(searchString));
                
                foreach (var skill in _context.UserSkills.Where(x => x.SkillName == searchString))
                {
                    foreach (AppUser user in UserMgr.Users.Where(x => x.Id == skill.UserID))
                    {
                        //This is to prevent duplicates
                        if (!userList.Contains(user))
                        {
                            userList.Add(user);
                        }
                    }
                }
                model.Users = userList;
            }

            if (!string.IsNullOrEmpty(SkillList))
            {
                items = items.Where(x => x.Skill == SkillList);
            }
          
            return View(model);

        }
    }
}