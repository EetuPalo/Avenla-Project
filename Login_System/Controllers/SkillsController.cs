using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SkillsController : Controller
    {
        private readonly SkillDataContext _context;
        private readonly UserSkillsDataContext userSkillsContext;
        private readonly SkillGoalContext goalContext;
        private UserManager<AppUser> UserMgr { get; }

        public SkillsController(SkillDataContext context, UserManager<AppUser> userManager, UserSkillsDataContext uSkillCon, SkillGoalContext skillGoalContext)
        {
            _context = context;
            UserMgr = userManager;
            userSkillsContext = uSkillCon;
            goalContext = skillGoalContext;
        }

        public async Task<IActionResult> Index (string searchString)
        {
            var skills = from c in _context.Skills select c;
            if (!String.IsNullOrEmpty(searchString))
            {
                skills = skills.Where(s => s.Skill.Contains(searchString));
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

        // GET: Skills/Create
        public IActionResult Create()
        {
            return View();
        }

        //Skill and SkillLevel are set by the user. User ID is set automatically based on the Id of the current user. SkillId is set in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Skill")] Skills skills)
        {
            //If the view is not valid, the user is just returned to the same view with error messages shown.
            if (ModelState.IsValid)
            {
                _context.Add(skills);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(skills);
        }

        // GET: Skills/Edit/5
        //This validates the ID that has been passed from the view, and if it's valid, it shows the edit view with the current user info already filled out.
        public async Task<IActionResult> Edit(int? id)
        {
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
            return View(skills);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, OldName, Skill")] Skills skills)
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
                //This here updates the skill name to the userskills table. similar loop should be run for every table that uses skillnames
                try
                {
                    foreach (var uSkill in userSkillsContext.UserSkills)
                    {
                        if (uSkill.SkillName == skills.OldName)
                        {
                            uSkill.SkillName = skills.Skill;
                            userSkillsContext.Update(uSkill);
                        }
                    }
                    await userSkillsContext.SaveChangesAsync();
                }
                catch
                {
                    return NotFound();
                }
                //This here updates the skill name to the skillgoals table.
                try
                {
                    foreach (var goal in goalContext.SkillGoals)
                    {
                        if (goal.SkillName == skills.OldName)
                        {
                            goal.SkillName = skills.Skill;
                            goalContext.Update(goal);
                        }
                    }
                    await goalContext.SaveChangesAsync();
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
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SkillsExists(int id)
        {
            return _context.Skills.Any(e => e.Id == id);
        }
    }
}
