using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Login_System.Controllers
{
    public class AdvancedSearchController : Controller
    {
        private readonly GeneralDataContext _context;

        public AdvancedSearchController(GeneralDataContext items)
        {
            _context = items;
            
        }
        public async Task<IActionResult> Index(string searchString)
        {
            var items = from i in _context.Skills select i;

            if (!String.IsNullOrEmpty(searchString))
            {
                items = items.Where(s => s.Skill.Contains(searchString));
            }
            return View(await items.ToListAsync());
        }
    }
}