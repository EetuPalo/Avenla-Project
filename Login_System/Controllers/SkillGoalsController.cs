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
using Login_System.ViewModels;

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
#nullable enable
        public async Task<IActionResult> Index(string name, string? date)
        {            
            if (name == null)
            {
                Console.WriteLine("No group selected. This is most likely an error.");
                return View();
            }
            string groupName = name;

            TempData["GroupName"] = groupName;
            TempData["Group"] = groupName;

            if (date == null && name != null)
            {
                var model = new SkillGoalIndexVM();
                var tempModel = new List<SkillGoals>();
                var modelCheck = new List<string>();

                foreach (var skillGoal in _context.SkillGoals)
                {
                    if (skillGoal.GroupName == groupName && !modelCheck.Contains(skillGoal.SkillName))
                    {
                        skillGoal.LatestGoal = GetLatest(skillGoal);
                        tempModel.Add(skillGoal);
                        modelCheck.Add(skillGoal.SkillName);
                    }
                }

                model.Goals = tempModel;
                model.SkillDates = GetDates(_context.SkillGoals);                

                if (model != null)
                {
                    return View(model);
                }
                Console.WriteLine("No entries have been found!");
                return View(_context.SkillGoals.ToList());
            }
            else if (date != null && name != null)
            {
                var model = new SkillGoalIndexVM();
                var tempModel = new List<SkillGoals>();
                var modelCheck = new List<string>();

                foreach (var skillGoal in _context.SkillGoals)
                {
                    if (skillGoal.GroupName == groupName && !modelCheck.Contains(skillGoal.SkillName) && skillGoal.Date.ToString("dd.MM.yyyy") == date)
                    {
                        skillGoal.LatestGoal = skillGoal.SkillGoal;
                        tempModel.Add(skillGoal);
                        modelCheck.Add(skillGoal.SkillName);
                    }
                }

                model.Goals = tempModel;
                model.SkillDates = GetDates(_context.SkillGoals);

                if (model != null)
                {
                    return View(model);
                }
                Console.WriteLine("No entries have been found!");
                return View(_context.SkillGoals.ToList());
            }

            return View();
        }
#nullable disable
        public async Task<IActionResult> RefreshIndex([Bind("GroupName, SelectedDate")]SkillGoalIndexVM goal)
        {
            return RedirectToAction(nameof(Index), "SkillGoals", new { name = goal.GroupName, date = goal.SelectedDate });
        }
        public async Task<IActionResult> ListByDate(string name)
        {
            if (name == null)
            {
                name = TempData["GroupName"].ToString();
            }

            var model = new List<SkillGoals>();

            foreach (var item in _context.SkillGoals)
            {
                if (item.SkillName == name)
                {
                    model.Add(item);
                }
            }
            TempData.Keep();
            return View(model);
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
        public IActionResult Create(string name)
        {
            var model = new CreateSkillGoalsVM();
            int dictKey = 0;
            model.SkillCounter = 0;

            var listModel = new Dictionary<int, SkillGoals>();

            foreach (var skill in skillContext.Skills)
            {
                var tempModel = new SkillGoals
                {
                    SkillName = skill.Skill,
                    GroupName = name
                };
                listModel.Add(dictKey, tempModel);
                dictKey++;
                model.SkillCounter++;
            }

            model.SkillGoals = listModel;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize("Admin")]
        public async Task<IActionResult> Create([Bind("SkillCounter,SkillGoals")] CreateSkillGoalsVM goals)
        {
            var model = new List<SkillGoals>();
            DateTime date = DateTime.Now;
            string dateMinute = date.ToString("dd.MM.yyyy");
            string groupName = TempData["Group"].ToString();
            TempData.Keep();

            //This is a complicated way to check if entries have already been made today. If a better way exists, we'll change this
            var todayList = new List<SkillGoals>();
            foreach (var goal in _context.SkillGoals)
            {
                if (goal.Date.ToString("dd.MM.yyyy") == dateMinute)
                {
                    todayList.Add(goal);
                }
            }

            if (todayList == null)
            {
                for (int i = 0; i < goals.SkillCounter; i++)
                {
                    try
                    {
                        var tempModel = new SkillGoals
                        {
                            GroupName = groupName,
                            SkillName = goals.SkillGoals[i].SkillName,
                            SkillGoal = goals.SkillGoals[i].SkillGoal,
                            Date = date
                        };
                        model.Add(tempModel);
                    }
                    catch
                    {
                        Console.WriteLine("Error occured at loop " + i);
                    }
                }

                foreach (var entry in model)
                {
                    _context.Add(entry);
                }
            }
            else if (todayList != null)
            {
                foreach (var goal in _context.SkillGoals)
                {
                    for (int i = 0; i < goals.SkillCounter; i++)
                    {
                        if (goal.SkillName == goals.SkillGoals[i].SkillName && goal.Date.ToString("dd.MM.yyyy") == dateMinute)
                        {
                            goal.SkillGoal = goals.SkillGoals[i].SkillGoal;
                            goal.Date = date;
                            _context.Update(goal);
                        }
                    }
                }
            }
           
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { name = TempData.Peek("GroupName") });
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

        public static List<SelectListItem> GetDates(DbSet<SkillGoals> skillList)
        {
            List<SelectListItem> ls = new List<SelectListItem>();
            var dateList = new List<string>();


            foreach (var item in skillList)
            {
                if (!dateList.Contains(item.Date.ToString("dd.MM.yyyy")))
                {
                    dateList.Add(item.Date.ToString("dd.MM.yyyy"));
                }               
            }
            foreach (var date in dateList)
            {
                ls.Add(new SelectListItem() { Text = date, Value = date });
            }

            return ls;
        }

        public int GetLatest(SkillGoals goal)
        {
            int currentGoal = 0;
            var dateList = new List<DateTime>();

            foreach (var skillGoal in _context.SkillGoals)
            {
                if (skillGoal.GroupName == goal.GroupName && skillGoal.SkillName == goal.SkillName)
                {
                    dateList.Add(skillGoal.Date);
                }
            }
            var dateResult = dateList.Max();

            foreach (var skillGoal in _context.SkillGoals)
            {
                if (skillGoal.GroupName == goal.GroupName && skillGoal.SkillName == goal.SkillName && skillGoal.Date == dateResult)
                {
                    currentGoal = skillGoal.SkillGoal;
                }
            }

            return currentGoal;
        }
    }
}
