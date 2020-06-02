using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SQLitePCL;

namespace Login_System.Controllers
{
    public class SkillCategoryController : Controller
    {
        private readonly GeneralDataContext _context;
        public SkillCategoryController(GeneralDataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name")] SkillCategories skillcat)
        {
            if (ModelState.IsValid)
            {
                _context.SkillCategories.Add(skillcat);
                await _context.SaveChangesAsync();
            }
           
            return View();
        }
    }
}