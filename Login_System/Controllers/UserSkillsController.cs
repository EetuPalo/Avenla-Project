using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Login_System.Controllers
{
    public class UserSkillsController : Controller
    {
        private readonly UserSkillsDataContext _context;
        private readonly SkillDataContext skillContext;
        private UserManager<AppUser> UserMgr { get; }

        public UserSkillsController(UserSkillsDataContext context, SkillDataContext sContext, UserManager<AppUser> userManager)
        {
            _context = context;
            skillContext = sContext;
            UserMgr = userManager;
        }

        // GET: UserSkills
        public async Task<IActionResult> Index(int id)
        {
            var model = new List<UserSkills>();
            foreach (var skill in _context.UserSkills)
            {
                //If the UserID of the skill is the same as the id that is passed from AppUser Index (that is the index of the current user), the skill is added to the list.
                if (skill.UserID == id || Convert.ToInt32(UserMgr.GetUserId(User)) == skill.UserID)
                {
                    model.Add(skill);
                }
            }

            return View(model);
        }

        // GET: UserSkills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSkills = await _context.UserSkills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userSkills == null)
            {
                return NotFound();
            }

            return View(userSkills);
        }

        // GET: UserSkills/Create
        public IActionResult Create()
        {
            var Skill = skillContext.Skills.ToList();
            var model = new UserSkills()
            {
                Skill = Skill.Select(x => new SelectListItem
                {
                    Value = x.Skill,
                    Text = x.Skill
                })
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SkillName, SkillLevel")] UserSkills userSkills)
        {
            if (ModelState.IsValid)
            {
                userSkills.Date = DateTime.Now;
                userSkills.UserID = Convert.ToInt32(UserMgr.GetUserId(User));

                _context.Add(userSkills);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userSkills);
        }

        // GET: UserSkills/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSkills = await _context.UserSkills.FindAsync(id);
            if (userSkills == null)
            {
                return NotFound();
            }
            return View(userSkills);
        }

        // POST: UserSkills/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserID,SkillID,SkillLevel,Date")] UserSkills userSkills)
        {
            if (id != userSkills.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userSkills);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserSkillsExists(userSkills.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(userSkills);
        }

        // GET: UserSkills/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSkills = await _context.UserSkills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userSkills == null)
            {
                return NotFound();
            }

            return View(userSkills);
        }

        // POST: UserSkills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userSkills = await _context.UserSkills.FindAsync(id);
            _context.UserSkills.Remove(userSkills);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserSkillsExists(int id)
        {
            return _context.UserSkills.Any(e => e.Id == id);
        }

        /*
        [HttpGet]
        public ActionResult ListSkills()
        {
            var skills = skillContext.Skills.ToList();
            var viewModel = new ListSkillsVM { Skills = skills };
            return View(viewModel);
        }
        */
    }
}
