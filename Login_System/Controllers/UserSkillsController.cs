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
using ASPNET_MVC_Samples.Models;
using Newtonsoft.Json;

namespace Login_System.Controllers
{
    public class UserSkillsController : Controller
    {
        private readonly UserSkillsDataContext _context;
        private readonly SkillDataContext skillContext;
        private readonly SkillGoalContext goalContext;
        private readonly GroupMembersDataContext gMemContext;
        private readonly GroupsDataContext groupContext;
        private UserManager<AppUser> UserMgr { get; }

        //These will be set in the index, and be used by other controller methods.
        public int userId;
        public string userName;

        public UserSkillsController(UserSkillsDataContext context, SkillDataContext sContext, UserManager<AppUser> userManager, SkillGoalContext gContext, GroupMembersDataContext memberContext, GroupsDataContext groupCon)
        {
            _context = context;
            skillContext = sContext;
            UserMgr = userManager;
            goalContext = gContext;
            gMemContext = memberContext;
            groupContext = groupCon;
        }

        public async Task<IActionResult> Index (int? id)
        {
            if (id == null)
            {
                id = Convert.ToInt32(TempData["UserId"]);
            }

            var model = new List<Skills>();

            foreach (var skill in skillContext.Skills)
            {
                /*
                skill.EntryCount = CountEntries(skill.Skill);
                try
                {
                    DateTime latestDateEntry = GetLatest(skill.Skill);
                    skill.LatestEntry = latestDateEntry.ToString("MM/dd/yyyy H:mm");
                    skill.LatestEval = GetLatestEval(skill.Skill, latestDateEntry);
                }
                catch
                {
                    skill.LatestEntry = "Not available!";
                    skill.LatestEval = 0;
                }
                */
                model.Add(skill);
            }
            //This is useful information we'll need in other controller actions
            //userId = id;
            var tempUser = await UserMgr.FindByIdAsync(id.ToString());
            //userName = tempUser.UserName;

            TempData["UserId"] = id;
            //TempData["UserName"] = tempUser.UserName;

            return View(model);
        }

        public async Task <IActionResult> ListByDate(int? id)
        {
            if (id == null)
            {
                id = Convert.ToInt32(TempData["UserId"]);
            }

            TempData["UserId"] = id;
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserName"] = tempUser.UserName;

            //var tempDate = new List<string>();
            
            var model = new List<DateListVM>();
            var tempDate = new List<string>();
            List<int> dataPoints = new List<int>();
            int i = 0;
            foreach (var item in _context.UserSkills)
            {
                if (item.UserID == id)
                {                    
                    if (!tempDate.Contains(item.Date.ToString()))
                    {
                        var tempModel = new DateListVM
                        {
                            Date = item.Date.ToString(),
                            AdminEval = item.AdminEval
                        };
                        model.Add(tempModel);
                    }
                    i += 10;
                    tempDate.Add(item.Date.ToString());
                    dataPoints.Add(item.SkillLevel);
                }                
            }
            ViewBag.DataPoint = JsonConvert.SerializeObject(dataPoints);
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SkillList(string date)
        {
            var model = new List<UserSkillsVM>();

            int userId = Convert.ToInt32(TempData["UserId"]);
            //string userName = TempData["UserName"].ToString();

            //Getting the skillgoal info for user group
            var skillGoalList = goalContext.SkillGoals.ToList();
            var groupMemberList = gMemContext.GroupMembers.ToList();
            var groupList = new List<Group>();
            var goalList = new List<GoalForSkillVM>();

            //Some complex stuff for fetching the correct skillgoal for the correct group for the correct skill for the correct user
            foreach (var member in groupMemberList)
            {
                if (member.UserID == userId)
                {
                    var tempGroup = new Group
                    {
                        id = member.GroupID,
                        name = member.GroupName
                    };

                    groupList.Add(tempGroup);
                }
            }

            foreach (var group in groupList)
            {
                foreach (var goal in skillGoalList)
                {
                    if (group.name == goal.GroupName)
                    {
                        var tempGoalList = new GoalForSkillVM();
                        tempGoalList.SkillName = goal.SkillName;
                        tempGoalList.SkillGoal = goal.SkillGoal;

                        goalList.Add(tempGoalList);
                    }
                }
            }

            AppUser tempUser = await UserMgr.FindByIdAsync(userId.ToString());
            TempData["UserName"] = tempUser.UserName;

            foreach (var skill in _context.UserSkills)
            {
                if (skill.Date.ToString() == date && skill.UserID == userId)
                {
                    var usrSkill = new UserSkillsVM();

                    usrSkill.Id = Convert.ToInt32(skill.Id);
                    usrSkill.UserID = skill.UserID;
                    usrSkill.UserName = userName;
                    usrSkill.SkillName = skill.SkillName;
                    usrSkill.SkillLevel = skill.SkillLevel;
                    usrSkill.Date = skill.Date.ToString("dd/MM/yyyy H:mm");
                    usrSkill.AdminEval = skill.AdminEval;

                    foreach (var goal in goalList)
                    {
                        if (skill.SkillName == goal.SkillName)
                        {
                            usrSkill.SkillGoal = goal.SkillGoal;
                        }
                    }

                    model.Add(usrSkill);
                }
            }

            TempData.Keep();
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
        public async Task<IActionResult> Create(int? id)
        {
            //var model = new List<UserSkillsWithSkillVM>();
            var tempModel = new UserSkillsWithSkillVM();
            var tempList = new Dictionary<int, string>();

            //Code here for creating the form based on the skillgoals (if 0, not part of the form)
            var skills = skillContext.Skills.ToList();
            var goals = goalContext.SkillGoals.ToList();
            var members = gMemContext.GroupMembers.ToList();
            var groups = groupContext.Group.ToList();

            var groupList = new List<Group>();
            var memberList = new List<GroupMember>();
            var skillList = new List<Skills>();
            var goalList = new List<SkillGoals>();
            var dateList = new List<DateTime>();

            //skillList.Add(skill);

            foreach (var member in members)
            {
                if (member.UserID == id)
                {
                    //Lists member entries relating to the current user
                    memberList.Add(member);
                }
            }
            foreach (var member in memberList)
            {
                foreach (var group in groups)
                {
                    if (member.GroupName == group.name)
                    {
                        groupList.Add(group);
                    }
                }
            }
            foreach (var group in groupList)
            {
                foreach (var goal in goals)
                {
                    if (goal.GroupName == group.name)
                    {
                        if (!dateList.Contains(goal.Date))
                        {
                            dateList.Add(goal.Date);
                        }
                        goalList.Add(goal);
                    }
                }
            }

            DateTime latestDate = dateList.Max();

            foreach (var goal in goalList)
            {
                foreach (var skill in skills)
                {
                    if (goal.SkillName == skill.Skill && goal.SkillGoal != 0 && goal.Date == latestDate)
                    {
                        skillList.Add(skill);
                    }
                }
            }


            int dictKey = 0;
            if (id == null)
            {
                id = TempData["UserId"] as int?;
                TempData.Keep();
            }
            foreach (var skill in skillList)
            {
                tempList.Add(dictKey, skill.Skill);
                dictKey++;
            }
            tempModel.SkillList = tempList;
            //model.Add(tempModel);
            return View(tempModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SkillList, SkillLevel, SkillCount")] UserSkillsWithSkillVM userSkills)
        {
            var model = new List<UserSkills>();

            //Date is declared here so that it's guaranteed to be the same for all skills.
            DateTime date = DateTime.Now;

            //Looping through the entries, adding them to a UserSkills object and adding that to a list.
            for (int i = 0; i < userSkills.SkillList.Count(); i++)
            {
                var tempModel = new UserSkills
                {
                    SkillLevel = userSkills.SkillLevel[i],
                    SkillName = userSkills.SkillList[i],
                    UserID = Convert.ToInt32(TempData["UserId"]),
                    Id = null,
                    Date = date               
                };

                if (User.IsInRole("Admin"))
                {
                    tempModel.AdminEval = "Admin Evaluation";
                }
                else
                {
                    tempModel.AdminEval = "Self Assessment";
                }

                model.Add(tempModel);
            }

            foreach (var entry in model)
            {
                _context.Add(entry);                
            }

            await _context.SaveChangesAsync();

            TempData.Keep();
            return RedirectToAction(nameof(ListByDate), "UserSkills", new { id = TempData.Peek("UserId") });

            /*
             _context.Add(userSkills);
             await _context.SaveChangesAsync();
             return RedirectToAction(nameof(Index), "UserSkills", new { id = model.UserID });
            */

            //return View(model);
        }

        // GET: UserSkills/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                id = TempData["UserId"] as int?;
            }

            var userSkills = await _context.UserSkills.FindAsync(id);
            if (userSkills == null)
            {
                return NotFound();
            }

            TempData.Keep();
            return View(userSkills);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AdminEval,UserID,SkillName,SkillLevel,Date")] UserSkills userSkills)
        {
            //int redirectId = userSkills.UserID;

            if (id != userSkills.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!userSkills.AdminEval.Contains(" - Edited"))
                    {
                        userSkills.AdminEval += (" - Edited");
                    }

                    _context.Update(userSkills);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserSkillsExists(Convert.ToInt32(userSkills.Id)))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("ListByDate", "UserSkills", new { id = userSkills.UserID });
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
            return RedirectToAction(nameof(ListByDate));
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

        //Below this are methods not currently used anywhere, but I'm keeping them here just in case.
        public int CountEntries(string skillName)
        {
            int entryCount = 0;

            int userId = Convert.ToInt32(TempData["UserId"]);
            foreach (var skill in _context.UserSkills)
            {
                if (skill.SkillName == skillName && skill.UserID == userId)
                {
                    entryCount++;
                }
            }

            TempData.Keep();
            return entryCount;
        }

        public DateTime GetLatest (string skillName)
        {
            DateTime latestDate;
            List<DateTime> allDates = new List<DateTime>();
            int userId = Convert.ToInt32(TempData["UserId"]);

            foreach (var skill in _context.UserSkills)
            {
                if (skill.SkillName == skillName && skill.UserID == userId)
                {
                    allDates.Add(skill.Date);
                }
            }
            latestDate = allDates.Max();
            TempData.Keep();
            return latestDate;
        }

        public int GetLatestEval (string skillName, DateTime latestDate)
        {
            int latestEval = 0;
            int userId = Convert.ToInt32(TempData["UserId"]);

            foreach (var skill in _context.UserSkills)
            {
                if (skill.SkillName == skillName && skill.UserID == userId && skill.Date == latestDate)
                {
                    latestEval = skill.SkillLevel;
                }
            }

            TempData.Keep();
            return latestEval;
        }
    }
}
