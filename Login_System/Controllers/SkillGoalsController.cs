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
        private readonly GroupMembersDataContext gMemContext;
        private readonly GroupsDataContext groupContext;
        private UserManager<AppUser> UserMgr { get; }

        public SkillGoalsController(SkillGoalContext context, UserManager<AppUser> userManager, SkillDataContext sContext, GroupMembersDataContext groupMemberCon, GroupsDataContext groupCon)
        {
            _context = context;
            UserMgr = userManager;
            skillContext = sContext;
            gMemContext = groupMemberCon;
            groupContext = groupCon;
        }

        // GET: SkillGoals
#nullable enable
        public IActionResult Index(string name, string? date)
        {            
            if (name == null)
            {
                Console.WriteLine("No group selected. This is most likely an error.");
                return View();
            }

            TempData["GroupName"] = name;
            TempData["Group"] = name;

            TempData["GroupID"] = groupContext.Group.Where(x => x.name == name).First().id;
            if (date == null && name != null)
            {
                //We can't pass a date if we are accessing the view from the groups index
                //So we have to get the latest date from the DB
                date = GetLatestDate(name).ToString("dd.MM.yyyy");
                TempData["LatestDate"] = GetLatestDate(name).ToString("dd.MM.yyyy");
            }

            if (date != null && name != null)
            {
                var model = new SkillGoalIndexVM();
                var tempModel = new List<SkillGoals>();
                var modelCheck = new List<string>();
                foreach (var skillGoal in _context.SkillGoals.Where(x => (x.GroupName == name)))
                {
                    if (!modelCheck.Contains(skillGoal.SkillName) && skillGoal.Date.ToString("dd.MM.yyyy") == date)
                    {
                        skillGoal.LatestGoal = skillGoal.SkillGoal;
                        tempModel.Add(skillGoal);
                        modelCheck.Add(skillGoal.SkillName);
                    }
                }

                model.Goals = tempModel;
                model.SkillDates = GetDates(_context.SkillGoals, name);
                model.DateToDelete = date;

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
        public IActionResult RefreshIndex([Bind("GroupName, SelectedDate")]SkillGoalIndexVM goal)
        {
            TempData["LatestDate"] = goal.SelectedDate;
            return RedirectToAction(nameof(Index), "SkillGoals", new { name = goal.GroupName, date = goal.SelectedDate });
        }
        public IActionResult ListByDate(string name)
        {
            if (name == null)
            {
                name = TempData["GroupName"].ToString();
            }

            var model = new List<SkillGoals>();
            foreach (var item in _context.SkillGoals.Where(x => x.SkillName == name))
            {
                model.Add(item);
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

            var listModel = new List<SkillGoals>();

            foreach (var skill in skillContext.Skills)
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
            model.Skills = skillContext.Skills.Select(x => new SelectListItem
            {
                Value = x.Skill,
                Text = x.Skill
            });
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize("Admin")]
        public async Task<IActionResult> Create(string source, [Bind("SkillCounter,SkillGoals")] CreateSkillGoalsVM goals)
        {
            var model = new List<SkillGoals>();
            DateTime date = DateTime.Now;
            string dateMinute = date.ToString("dd.MM.yyyy");
            string groupName = goals.SkillGoals[0].GroupName;
            int groupId = 0;
            TempData.Keep();

            var duplicateCheck = new List<string>();

            //This is a complicated way to check if entries have already been made today
            var todayList = new List<SkillGoals>();

            foreach (var goal in _context.SkillGoals.Where(x => x.GroupName == groupName))
            {
                if (goal.Date.ToString("dd.MM.yyyy") == dateMinute)
                {
                    todayList.Add(goal);
                }
            }
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
            foreach (var skill in skillContext.Skills)
            {
                var index = model.FindIndex(item => item.SkillName == skill.Skill);
                if (index == -1)
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
                //This displays a warning if a skill has been selected more than once
                if (duplicateCheck.Contains(entry.SkillName))
                {
                    TempData["ActionWarning"] = "A skill has been selected multiple times. One of the selections may have been overridden.";
                }
                duplicateCheck.Add(entry.SkillName);
                _context.Add(entry);
            }

            if (todayList != null)
            {
                foreach (var skillGoal in _context.SkillGoals.Where(x => x.GroupName == groupName))
                {
                    foreach (var todayEntry in todayList.Where(x => (x.Date == skillGoal.Date)))
                    {
                        _context.Remove(skillGoal);
                    }
                }
            }
            await _context.SaveChangesAsync();
            
            foreach (var group in groupContext.Group.Where(x => x.name == groupName))
            {
                groupId = group.id;
            }

            TempData["ActionResult"] = "New goals set!";
            if (source == "create")
            {
                TempData["ActionResult"] = "Goals set! Now you can add users to the group.";
                TempData["ActionPhase"] = "[3/3]";
                return RedirectToAction(nameof(Create), "GroupMembers", new { source = "create", id =  groupId, group = groupName});
            }
            return RedirectToAction(nameof(Index), new { name = groupName});
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
        public async Task<IActionResult> Edit(int id, [Bind("Id, SkillName, GroupName, SkillGoal, Date")] SkillGoals skillGoals)
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

        public async Task<IActionResult> DeleteForm(string date, string group)
        {
            if (date == null || group == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    foreach (var entry in _context.SkillGoals.Where(x => x.GroupName == group))
                    {
                        if (entry.Date.ToString("dd.MM.yyyy") == date)
                        {
                            _context.Remove(entry);
                        }
                    }
                    await _context.SaveChangesAsync();
                    TempData["ActionResult"] = "Goals deleted successfully!";
                }
                catch
                {
                    TempData["ActionResult"] = "An exception occured when deleting goals!";
                }
            }
            return RedirectToAction(nameof(Index), new { name = group });
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

            foreach (var item in skillList.Where(x => x.GroupName == groupName))
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

        public DateTime GetLatestDate(string group)
        {
            var dateList = new List<DateTime>();
            DateTime maxDate;
            foreach (var goal in _context.SkillGoals.Where(x => (x.GroupName == group)))
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

            foreach (var skillGoal in _context.SkillGoals.Where(x => (x.GroupName == goal.GroupName) && (x.SkillName == goal.SkillName)))
            {
                dateList.Add(skillGoal.Date);
            }
            var dateResult = dateList.Max();
            foreach (var skillGoal in _context.SkillGoals.Where(x => (x.GroupName == goal.GroupName) && (x.SkillName == goal.SkillName) && (x.Date == dateResult)))
            {
                currentGoal = skillGoal.SkillGoal;
            }
            return currentGoal;
        }
    }
}
