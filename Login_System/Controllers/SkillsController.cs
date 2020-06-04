using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;
using Login_System.ViewModels;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin, Superadmin")]
    public class SkillsController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        public SkillsController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }

        public async Task<IActionResult> Index (string searchString)
        {
            //Select all skills
            var skills = from c in _context.Skills select c;
            TempData["SearchValue"] = null;

            if (!String.IsNullOrEmpty(searchString))
            {
                //Select only those skills that contain the searchString
                skills = skills.Where(s => s.Skill.Contains(searchString));
                TempData["SearchValue"] = searchString;
            }
            return View(await skills.ToListAsync());
        }

        // GET: Skills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skills = await _context.Skills.FirstOrDefaultAsync(m => m.Id == id);

            if (skills == null)
            {
                return NotFound();
            }

            return View(skills);
        }

        //GET: Skills/Create
        public IActionResult Create()
        {
            var model = new SkillCreateVM();
            var tempList = new List<SkillCategories>();
            foreach (var item in _context.SkillCategories)
            {
                model.SkillCategoryList.Add(new SelectListItem() { Text = item.Name, Value = item.Name});
                tempList.Add(item);
            }
            model.ListOfCategories = tempList;
            return View(model);
        }

        //Skill and SkillLevel are set by the user. User ID is set automatically based on the Id of the current user. SkillId is set in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Skill, Description")] Skills skills, string[] SkillCategory)
        {
            //If the view is not valid, the user is just returned to the same view with error messages shown.
            if (ModelState.IsValid)
            {

                //SkillCategories skillCategories = new SkillCategories();
                _context.Add(skills);

              
                await _context.SaveChangesAsync();
                foreach(var skillcategories in SkillCategory)
                {
                    var category = await _context.SkillCategories.FirstOrDefaultAsync(x => x.Name == skillcategories);
                    SkillsInCategory skill = new SkillsInCategory
                    {
                        SkillId = skills.Id,
                        CategoryId = category.id

                    };
                    _context.SkillsInCategory.Add(skill);
                }
         
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(skills);
        }

        //GET: Skills/Edit/5
        //This validates the ID that has been passed from the view, and if it's valid, it shows the edit view with the current user info already filled out.
        public async Task<IActionResult> Edit(int? id)
        {
            SkillCreateVM model = new SkillCreateVM();
            
            if (id == null)
            {
                return NotFound();
            }

            var skills = await _context.Skills.FindAsync(id);
            skills.OldName = skills.Skill;

            if (skills == null)
            {
                return NotFound();
            }
            var tempList = new List<SkillCategories>();
            foreach (var item in _context.SkillCategories)
            {
                model.SkillCategoryList.Add(new SelectListItem() { Text = item.Name, Value = item.Name });
                tempList.Add(item);
            }

            model.skill = skills;
            model.ListOfCategories = tempList;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, OldName, Skill")] Skills skills,  string[] SkillCategory)
        {
            if (id != skills.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skills);
                    foreach(var skillincategory in _context.SkillsInCategory.Where(x=> x.SkillId == skills.Id))
                    {
                        _context.SkillsInCategory.Remove(skillincategory);
                    }
                    await _context.SaveChangesAsync();


                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillsExists(skills.Id))
                    {
                        return NotFound();
                    }

                    else
                    {
                        throw;
                    }
                }
                //This updates the skill name to the userskills table. similar loop should be run for every table that uses skillnames
                try
                {
                    foreach (var uSkill in _context.UserSkills.Where(x => x.SkillName == skills.OldName))
                    {
                        uSkill.SkillName = skills.Skill;
                        _context.Update(uSkill);
                    }
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    return NotFound();
                }
                //This updates the skill name to the skillgoals table.
                try
                {
                    foreach (var goal in _context.SkillGoals.Where(x => x.SkillName == skills.OldName))
                    {
                        goal.SkillName = skills.Skill;
                        _context.Update(goal);
                    }
                    await _context.SaveChangesAsync();
                }
            
                catch
                {
                    return NotFound();
                } 
                try
                {
                    foreach (var skillcategories in SkillCategory)
                    {
                        var category = await _context.SkillCategories.FirstOrDefaultAsync(x => x.Name == skillcategories);
                        SkillsInCategory skill = new SkillsInCategory
                        {
                            SkillId = skills.Id,
                            CategoryId = category.id

                        };
                        _context.SkillsInCategory.Add(skill);
                        await _context.SaveChangesAsync();
                    }
                }
                catch
                {
                    return NotFound();
                }
                //
                return RedirectToAction(nameof(Index));
            }
            return View(skills);
        }

        // GET: Skills/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skills = await _context.Skills
                .FirstOrDefaultAsync(m => m.Id == id);

            if (skills == null)
            {
                return NotFound();
            }

            return View(skills);
        }

        // POST: Skills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skills = await _context.Skills.FindAsync(id);
            _context.Skills.Remove(skills);

            var categories = await _context.SkillsInCategory.Where(x => x.SkillId == skills.Id).ToListAsync();
            foreach(var item in categories)
            {
                _context.SkillsInCategory.Remove(item);
            }
     
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool SkillsExists(int id)
        {
            return _context.Skills.Any(e => e.Id == id);
        }
    }
}
