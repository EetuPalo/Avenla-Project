using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Identity;
using ASPNET_MVC_Samples.Models;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace Login_System.Controllers
{
    [Authorize(Roles = "User, Admin, Superadmin")]
    public class UserGoalsController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        int uId;

        //These will be set in the index, and be used by other controller methods.
        public int userId;
        public string userName;

        public UserGoalsController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }

        // GET: UserGoals
        public async Task<IActionResult> Index(int? id, string searchString, string source)
        {
            if (id == null)
            {
                id = Convert.ToInt32(UserMgr.GetUserId(User));
            }

            TempData["UserId"] = id;

            if (source != null)
            {
                TempData["Source"] = source;
            }

            else
            {
                TempData["Source"] = "appuser";
            }

            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserName"] = tempUser.FirstName + " " + tempUser.LastName;
            TempData.Keep();

            var goals = from c in _context.UserGoals.Where(c => c.UserId == id) select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                goals = goals.Where(s => (s.SkillName.Contains(searchString)));
            }
            return View(await goals.ToListAsync());
        }


        [HttpGet]
        public IActionResult Create(int? id)
        {
            if (id == null || id == 0)
            {
                id = Convert.ToInt32(UserMgr.GetUserId(User));
            }
            TempData["UserId"] = id;

            var model = new UserGoalsVM();

            var curListUserGoals = new List<int>();
            foreach (var userGoals in _context.UserGoals.Where(x => x.UserId == id))
            {
                curListUserGoals.Add(userGoals.SkillId);
            }

            foreach (var skill in _context.Skills)
            {
                if(!curListUserGoals.Contains(skill.Id))
                {
                    model.AvailableSkillList.Add(new SelectListItem() { Value = skill.Id.ToString(), Text = skill.Skill });
                }

            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SkillId, SkillGoal")] UserGoalsVM goal)
        {
            if (ModelState.IsValid)
            {
                var newGoal = new UserGoals();

                newGoal.UserId = Convert.ToInt32(TempData["UserId"]);
                newGoal.SkillId = goal.SkillId;
                newGoal.SkillGoal = goal.SkillGoal;
                newGoal.SkillName = _context.Skills.Where(x => x.Id == goal.SkillId).FirstOrDefault().Skill;

                _context.UserGoals.Add(newGoal);

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(goal);
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var userGoal = await _context.UserGoals.FindAsync(id);
            if (userGoal == null)
            {
                return NotFound();
            }
            _context.UserGoals.Remove(userGoal);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userGoal = await _context.UserGoals.FindAsync(id);
            if (userGoal == null)
            {
                return NotFound();
            }
            return View(userGoal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SkillGoal")] UserGoals editUserGoal)
        {
            if (id != editUserGoal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                foreach( var userGoal in _context.UserGoals.Where(x => x.Id == editUserGoal.Id))
                {
                    userGoal.SkillGoal = editUserGoal.SkillGoal;
                    _context.Update(userGoal);
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(editUserGoal);
        }

    }
}
