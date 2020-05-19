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

namespace Login_System.Controllers
{
    public class AdvancedSearchController : Controller
    {
        private readonly GeneralDataContext _context;

        public AdvancedSearchController(GeneralDataContext context)
        {
            _context = context;
            
        }
        public async Task<IActionResult> Index(string searchString, string SkillList)
        {
            var model = new AdvancedSearchVM();

            IQueryable<string> SkillQuery = from s in _context.Skills
                                            orderby s.Skill
                                            select s.Skill;
            var items = from m in _context.Skills
                        select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                items = items.Where(s => s.Skill.Contains(searchString));
                
            }

            if (!string.IsNullOrEmpty(SkillList))
            {
                items = items.Where(x => x.Skill == SkillList);
            }
 
            var appUser = new AppUser();

            return View(model);

        }
    }
}