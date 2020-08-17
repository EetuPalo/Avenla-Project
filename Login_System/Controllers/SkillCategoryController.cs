using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Login_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> Index(string searchString)
        {
            //Select all skills
            var skills = from c in _context.SkillCategories select c;
            TempData["SearchValue"] = null;

            if (!String.IsNullOrEmpty(searchString))
            {
                //Select only those skills that contain the searchString
                skills = skills.Where(s => s.Name.Contains(searchString));
                TempData["SearchValue"] = searchString;
            }
            return View(await skills.ToListAsync());
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
                var model = new SkillCategories();
                _context.SkillCategories.Add(skillcat);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
           
            return View();
        }
        [Authorize(Roles = "Admin, Superadmin")]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var model = new SkillCategories();
            model.Name = _context.SkillCategories.FirstOrDefault(x=> x.id == id).Name;
            return View(model);

        }
        [Authorize(Roles = "Admin, Superadmin")]
        [HttpPost]
        public async Task<IActionResult> Delete(SkillCategories skillcat)
        {
            if (ModelState.IsValid)
            {
                _context.SkillCategories.Remove(skillcat);
                _context.SkillsInCategory.RemoveRange(_context.SkillsInCategory.Where(x => x.SkillId == skillcat.id));
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}