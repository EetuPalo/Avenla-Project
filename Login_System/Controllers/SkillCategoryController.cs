using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Login_System.Models;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SQLitePCL;

namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin, Superadmin")]
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

        [HttpGet]
        public IActionResult Create()
        {
            var model = new SkillCategoryVM();

            //filling a dropdown for available skills
            foreach (var skill in _context.Skills)
            {
                model.SkillList.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Id.ToString() });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id, Name, Description, Skills")] SkillCategoryVM skillcat)
        {
            if (ModelState.IsValid)
            {
                var category = _context.SkillCategories.Add(new SkillCategories
                {
                    Name = skillcat.Name,
                    Description = skillcat.Description,
                }).Entity;
                await _context.SaveChangesAsync();

                foreach (var skill in skillcat.Skills)
                {
                    _context.SkillsInCategory.Add(new SkillsInCategory
                    {
                        SkillId = skill,
                        CategoryId = category.id
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(skillcat);
        }

        [Authorize(Roles = "Admin, Superadmin")]
        [HttpDelete]
        public async Task <IActionResult> Delete(int id)
        {
            var skillcat = new SkillCategories();
            skillcat = await _context.SkillCategories.FindAsync(id);

            if (ModelState.IsValid)
            {
                _context.SkillCategories.Remove(skillcat);
                _context.SkillsInCategory.RemoveRange(_context.SkillsInCategory.Where(x => x.CategoryId == skillcat.id));
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Delete successful" });
            }

            return Json(new { success = false, message = "Delete not successful" });
        }
        //[Authorize(Roles = "Admin, Superadmin")]
        //[HttpGet]
        //public ActionResult Delete(int id)
        //{
        //    var model = new SkillCategories();
        //    model.Name = _context.SkillCategories.FirstOrDefault(x => x.id == id).Name;
        //    return View(model);
        //}

        //[Authorize(Roles = "Admin, Superadmin")]
        //[HttpPost]
        //public async Task<IActionResult> Delete(SkillCategories skillcat)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.SkillCategories.Remove(skillcat);
        //        _context.SkillsInCategory.RemoveRange(_context.SkillsInCategory.Where(x => x.CategoryId == skillcat.id));
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View();
        //}

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            var model = new SkillCategoryVM();
            var skillcat = await _context.SkillCategories.FirstOrDefaultAsync(x => x.id == id);
            List<Skills> skillList = new List<Skills>();
            var skills = _context.SkillsInCategory.Where(x => x.CategoryId == id).ToList();

            foreach (var item in skills)
            {
                skillList.Add(await _context.Skills.FirstOrDefaultAsync(x => x.Id == item.SkillId));
            }
            model.SkillsInCategory = skillList;
            model.Description = skillcat.Description;
            model.Name = skillcat.Name;
            model.Id = skillcat.id;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = new SkillCategoryVM();
            var skillcat = await _context.SkillCategories.FirstOrDefaultAsync(x => x.id == id);
            foreach (var skill in _context.Skills)
            {
                model.SkillList.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Id.ToString() });
            }

            model.currentSkills = _context.SkillsInCategory.Where(x => x.CategoryId == id).Select(x => x.SkillId.ToString()).ToList();
            model.Id = id;
            model.Name = skillcat.Name;
            model.Description = skillcat.Description;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind ("Id,Name,Description,Skills")] SkillCategoryVM skillCat)
        {
            //first we edit the category itself
            var category = await _context.SkillCategories.FirstOrDefaultAsync(x => x.id == skillCat.Id);
            category.Description = skillCat.Description;
            category.Name = skillCat.Name;
            
            _context.Update(category);
            // then we edit the skills that are included in the category
            foreach (var item in _context.SkillsInCategory.Where(x => x.CategoryId == skillCat.Id))
            {
                if (!skillCat.Skills.Contains(item.SkillId))
                {
                    _context.SkillsInCategory.Remove(item);
                }
            }
            await _context.SaveChangesAsync();
            foreach (var newItem in skillCat.Skills)
            {
                var skill = new SkillsInCategory
                {
                    SkillId = newItem,
                    CategoryId = skillCat.Id
                };
                if (!_context.SkillsInCategory.Any(x => x.CategoryId == skill.CategoryId && x.SkillId == skill.SkillId))
                {
                    _context.SkillsInCategory.Add(skill);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = skillCat.Id });
        }
    }
}