using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Login_System.Controllers
{
    public class SkillGoalsController : Controller
    {
        private readonly SkillGoalContext _context;
        private readonly SkillDataContext skillContext;
        private UserManager<AppUser> UserMgr { get; }

        public SkillGoalsController(SkillGoalContext context, UserManager<AppUser> userManager, SkillDataContext sContext)
        {
            _context = context;
            UserMgr = userManager;
            skillContext = sContext;
        }

        // GET: SkillGoals
        public async Task<IActionResult> Index(string name)
        {
            if (name == null)
            {
                Console.WriteLine("No group selected. This is most likely an error.");
                return View();
            }
            var model = new List<SkillGoals>();

            foreach (var skillGoal in _context.SkillGoals)
            {
                if (skillGoal.GroupName == name)
                {
                    model.Add(skillGoal);
                }
            }

            TempData["GroupName"] = name;
            if (model != null)
            {
                return View(model);
            }
            Console.WriteLine("No entries have been found!");
            return View(_context.SkillGoals.ToList());
        }

        // GET: SkillGoals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillGoals = await _context.SkillGoals
                .FirstOrDefaultAsync(m => m.Id == id);
            if (skillGoals == null)
            {
                return NotFound();
            }

            return View(skillGoals);
        }

        // GET: SkillGoals/Create
        public IActionResult Create()
        {
            var model = new SkillGoals();
            var skillList = skillContext.Skills;
            model.Skills = GetItems(skillList);



            return View(model);
        }

        // POST: SkillGoals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize("Admin")]
        public async Task<IActionResult> Create([Bind("SkillName,SkillGoal")] SkillGoals skillGoals)
        {
            if (ModelState.IsValid)
            {
                skillGoals.GroupName = TempData["GroupName"].ToString();
                TempData.Keep();

                _context.Add(skillGoals);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { name = TempData.Peek("GroupName")});
            }
            return View(skillGoals);
        }

        // GET: SkillGoals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            string groupName = TempData["GroupName"].ToString();
            TempData.Keep();

            if (id == null)
            {
                return NotFound();
            }

            var skillGoals = await _context.SkillGoals.FindAsync(id);
            if (skillGoals == null)
            {
                return NotFound();
            }
            ViewBag.GroupName = groupName;
            return View(skillGoals);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, SkillName, GroupName, SkillGoal")] SkillGoals skillGoals)
        {
            if (id != skillGoals.Id)
            {

                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skillGoals);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillGoalsExists(skillGoals.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { name = TempData.Peek("GroupName") });
            }
            return View(skillGoals);
        }

        // GET: SkillGoals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillGoals = await _context.SkillGoals
                .FirstOrDefaultAsync(m => m.Id == id);
            if (skillGoals == null)
            {
                return NotFound();
            }

            return View(skillGoals);
        }

        // POST: SkillGoals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skillGoals = await _context.SkillGoals.FindAsync(id);
            _context.SkillGoals.Remove(skillGoals);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { name = TempData.Peek("GroupName") });
        }

        private bool SkillGoalsExists(int id)
        {
            return _context.SkillGoals.Any(e => e.Id == id);
        }

        public static List<SelectListItem> GetItems(DbSet<Skills> skillList)
        {
            List<SelectListItem> ls = new List<SelectListItem>();
            foreach (var item in skillList)
            {
                ls.Add(new SelectListItem() { Text = item.Skill, Value = item.Skill });
            }
            return ls;
        }
    }
}
