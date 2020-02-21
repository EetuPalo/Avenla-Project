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

            TempData["GroupName"] = name;
            TempData["Group"] = name;

            //This gets us the latest date an entry has been made and displays it at the top of the page
            if (TempData["LatestDate"] == null)
            {
                var latestDate = GetLatestDate().ToString("dd.MM.yyyy");
                TempData["LatestDate"] = latestDate;
            }

            if (date == null && name != null)
            {
                var model = new SkillGoalIndexVM();
                var tempModel = new List<SkillGoals>();
                var modelCheck = new List<string>();

                foreach (var skillGoal in _context.SkillGoals)
                {
                    if (skillGoal.GroupName == name && !modelCheck.Contains(skillGoal.SkillName))
                    {
                        skillGoal.LatestGoal = GetLatest(skillGoal);
                        tempModel.Add(skillGoal);
                        modelCheck.Add(skillGoal.SkillName);
                    }
                }

                model.Goals = tempModel;
                model.SkillDates = GetDates(_context.SkillGoals, name);
                model.GroupName = name;

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
                    if (skillGoal.GroupName == name && !modelCheck.Contains(skillGoal.SkillName) && skillGoal.Date.ToString("dd.MM.yyyy") == date)
                    {
                        skillGoal.LatestGoal = skillGoal.SkillGoal;
                        tempModel.Add(skillGoal);
                        modelCheck.Add(skillGoal.SkillName);
                    }
                }

                model.Goals = tempModel;
                model.SkillDates = GetDates(_context.SkillGoals, name);

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
            TempData["LatestDate"] = goal.SelectedDate;
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
            DateTime date = DateTime.Now;
            int dictKey = 0;
            model.SkillCounter = 0;

            var skills = skillContext.Skills.ToList();
            var listModel = new List<SkillGoals>();

            foreach (var skill in skills)
            {
                var tempModel = new SkillGoals
                {
                    SkillName = skill.Skill,
                    GroupName = name
                };
               
                listModel.Add(tempModel);
                dictKey++;
                model.SkillCounter++;
            }
            model.SkillGoals = listModel;
            model.Skills = skills.Select(x => new SelectListItem
            {
                Value = x.Skill,
                Text = x.Skill
            });
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

            //This is a complicated way to check if entries have already been made today
            var todayList = new List<SkillGoals>();
            var skillList = skillContext.Skills.ToList();

            foreach (var goal in _context.SkillGoals)
            {
                if (goal.Date.ToString("dd.MM.yyyy") == dateMinute && goal.GroupName == groupName)
                {
                    todayList.Add(goal);
                }
            }
            if(true)
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
                foreach (var skill in skillList)
                {
                    var skillName = model.FindIndex(item => item.SkillName == skill.Skill);
                    if (skillName == -1)
                    {
                        var tempModel = new SkillGoals
                        {
                            GroupName = groupName,
                            SkillName = skill.Skill,
                            SkillGoal = -1,
                            Date = date
                        };
                        model.Add(tempModel);
                    }
                }
                var negModel = new List<SkillGoals>();
                var plusModel = new List<SkillGoals>();
                var combModel = new List<SkillGoals>();
                foreach (var entry in model)
                {
                    if (entry.SkillGoal == -1)
                    {
                        negModel.Add(entry);
                    }
                    else if (entry.SkillGoal >= 0)
                    {
                        plusModel.Add(entry);
                    }
                }
                foreach (var negEntry in negModel)
                {
                    var index = plusModel.FindIndex(item => item.SkillName == negEntry.SkillName);
                    if (index == -1)
                    {
                        combModel.Add(negEntry);
                    }
                }
                foreach (var entry in plusModel)
                {
                    combModel.Add(entry);
                }

                foreach (var entry in combModel)
                {
                    _context.Add(entry);
                }

                if (todayList != null)
                {
                    foreach (var skillGoal in _context.SkillGoals)
                    {
                        foreach (var todayEntry in todayList)
                        {
                            if (skillGoal.Date == todayEntry.Date && skillGoal.GroupName == groupName)
                            {
                                _context.Remove(skillGoal);
                            }
                        }
                    }
                }
            }           
            await _context.SaveChangesAsync();
            TempData["ActionResult"] = "New goals set!";
            return RedirectToAction(nameof(Index), new { name = TempData.Peek("GroupName")});
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
                TempData["ActionResult"] = "Goals edited successfully!";
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

            TempData["ActionResult"] = "Goals deleted successfully!";
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

        //This takes the list of skills and groupname to put all the dates where skillgoal entries have been made (for that specific group) into a list
        public static List<SelectListItem> GetDates(DbSet<SkillGoals> skillList, string groupName)
        {
            List<SelectListItem> ls = new List<SelectListItem>();
            var dateList = new List<string>();


            foreach (var item in skillList)
            {
                if (!dateList.Contains(item.Date.ToString("dd.MM.yyyy")) && item.GroupName == groupName)
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

        public DateTime GetLatestDate()
        {
            var goalList = _context.SkillGoals.ToList();
            var dateList = new List<DateTime>();
            DateTime maxDate;
            foreach (var goal in goalList)
            {
                if (!dateList.Contains(goal.Date))
                {
                    dateList.Add(goal.Date);
                }
            }
            if (dateList.Count() != 0)
            {
                maxDate = dateList.Max();
            }
            else
            {
                maxDate = DateTime.Now;
            }
            return maxDate;
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
