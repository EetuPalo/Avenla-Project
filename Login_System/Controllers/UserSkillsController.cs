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

        int uId;

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
                id = Convert.ToInt32(TempData.Peek("UserId"));
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
            TempData["UserId"] = id;
            TempData["UserName"] = tempUser.UserName;

            return View(model);
        }

        public async Task <IActionResult> ListByDate(int? id, string searchString)
        {
            if (id == null)
            {
                //I dont understand tempdata lol
                id = Convert.ToInt32(TempData.Peek("UserId"));
                TempData.Keep();                
            }
            else
            {
                TempData["UserId"] = id;
            }

            //if it's still null
            if (id == null || id == 0)
            {
                id = Convert.ToInt32(UserMgr.GetUserId(User));
            }

            uId = (int)id;
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            //string userName = tempUser.UserName;
            TempData["UserName"] = tempUser.UserName;
            ViewBag.UserNames = tempUser.FirstName + " " + tempUser.LastName;

            var model = new List<DateListVM>();
            var tempDate = new List<string>();
            List<DataPoint> dataPoints = new List<DataPoint>();
            //List<DateTime> dates = new List<DateTime>();
            List<string> dates = new List<string>();
            List<string> skillnames = new List<string>();
            int i = 0;

            var userSkills = from c in _context.UserSkills select c;
            if (!String.IsNullOrEmpty(searchString))
            {
                userSkills = userSkills.Where(s => ((s.Date.Day.ToString().Contains(searchString)) || (s.Date.Month.ToString().Contains(searchString)) || (s.Date.Year.ToString().Contains(searchString)) || (s.Date.Hour.ToString().Contains(searchString)) || (s.Date.Minute.ToString().Contains(searchString))) && (s.UserID == id));
                foreach (var item in userSkills)
                {
                    if (item.UserID == id)
                    {
                        if (!tempDate.Contains(item.Date.ToString()))
                        {
                            i++;
                            var tempModel = new DateListVM
                            {
                                Date = item.Date.ToString("dd.MM.yyyy HH:mm"),
                                AdminEval = item.AdminEval,
                                TempDate = item.Date.ToString("dd/MM/yyyy+HH/mm"),
                                Id = (int)id
                            };
                            model.Add(tempModel);

                        }
                        tempDate.Add(item.Date.ToString());      
                        if(item.Date.Month == DateTime.Now.Month && item.Date.Year == DateTime.Now.Year)
                        {
                            skillnames.Add(item.SkillName);
                            dates.Add(item.Date.ToString("dd.MM.yyyy.HH.mm.ss")); 
                            dataPoints.Add(new DataPoint(item.Date.Day, item.SkillLevel));
                        }
                            
                    }
                }
            }
            else
            {
                foreach (var item in _context.UserSkills)
                {
                    if (item.UserID == id)
                    {
                        if (!tempDate.Contains(item.Date.ToString()))
                        {
                            i++;
                            var tempModel = new DateListVM
                            {
                                Date = item.Date.ToString("dd.MM.yyyy HH:mm"),
                                AdminEval = item.AdminEval,
                                TempDate = item.Date.ToString("dd/MM/yyyy+HH/mm"),
                                Id = (int)id
                            };
                            model.Add(tempModel);

                        }
                        tempDate.Add(item.Date.ToString());

                        if (item.Date.Month == DateTime.Now.Month && item.Date.Year == DateTime.Now.Year)
                        {
                            skillnames.Add(item.SkillName);
                            dates.Add(item.Date.ToString("dd.MM.yyyy.HH.mm.ss"));
                            dataPoints.Add(new DataPoint(item.Date.Day, item.SkillLevel));
                        }
                    }
                }
            }

           
            ViewBag.DataPoint = dataPoints.ToArray();
            //ViewBag.Dates = JsonConvert.SerializeObject(dates);
            ViewBag.Dates = dates.ToArray();
            ViewBag.names = skillnames.ToArray();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SkillList(string name, int? id)
        {
            if (id == null)
            {
                id = Convert.ToInt32(UserMgr.GetUserId(User));
            }

            var model = new List<UserSkillsVM>();

            int userId = (int)id;
            TempData.Keep();
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            string userName = tempUser.UserName;
            TempData["UserName"] = userName;
            TempData["UserId"] = id;
            string tempName = "DATE_NOT_FOUND";

            //Getting the skillgoal info for user group
            var skillGoalList = goalContext.SkillGoals.ToList();
            var groupMemberList = gMemContext.GroupMembers.ToList();
            var groupList = new List<Group>();
            var goalList = new List<GoalForSkillVM>();
            var dateList = new Dictionary<string, string>();

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
                        var tempGoalList = new GoalForSkillVM
                        {
                            SkillName = goal.SkillName,
                            SkillGoal = goal.SkillGoal,
                            Date = goal.Date.ToString(),
                            GroupName = goal.GroupName
                        };
                        goalList.Add(tempGoalList);
                    }
                }
            }

            //If the skilllist is accessed directly from AppUser Index, the latest entries are automatically shown.
            if (name == "latest")
            {
                var tempDateList = new List<DateTime>();
                foreach (var skill in _context.UserSkills)
                {
                    if (!tempDateList.Contains(skill.Date))
                    {
                        tempDateList.Add(skill.Date);
                    }
                }
                name = tempDateList.Max().ToString("dd/MM/yyyy+HH/mm");
                tempName = tempDateList.Max().ToString("dd.MM.yyyy HH.mm");
                TempData["Date"] = tempName;
            }

            

            foreach (var skill in _context.UserSkills)
            {               
                var date1 = skill.Date.ToString("dd/MM/yyyy+HH/mm");
                var date2 = name;
                var skillGoal = 0;

                if (date1 == date2 && skill.UserID == userId)
                {
                    tempName = skill.Date.ToString("dd.MM.yyyy HH.mm");
                    TempData["Date"] = tempName;

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
                        foreach (var group in groupList)
                        {
                            if (goal.SkillGoal > -1 && goal.GroupName == group.name)
                            {
                                if (dateList.ContainsKey(group.name))
                                {
                                    dateList[group.name] = goal.Date;
                                }
                                else if (!dateList.ContainsKey(group.name))
                                {
                                    dateList.Add(group.name, goal.Date);
                                }
                            }
                        }                        
                    }     
                    
                    foreach (var date in dateList.Values)
                    {
                        foreach (var goal in goalList)
                        {
                            if (date == goal.Date && goal.SkillName == skill.SkillName && goal.SkillGoal > skillGoal)
                            {
                                skillGoal = goal.SkillGoal;
                            }
                        }
                    }

                    usrSkill.SkillGoal = skillGoal;
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
        public async Task<IActionResult> Create(int id)
        {
            TempData["UserId"] = id;
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
            var dateList = new Dictionary<string, DateTime>();

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
                        DateTime value = goal.Date;
                        if (dateList.ContainsKey(group.name))
                        {
                            dateList[group.name] = value;
                        }
                        else
                        {
                            dateList.Add(group.name, value);
                        }

                        goalList.Add(goal);
                    }
                }
            }
            DateTime latestDate = DateTime.Now;
            var latestDateList = new List<DateTime>();
            if (dateList.Count() != 0)
            {
                foreach (var key in dateList.Keys)
                {
                    latestDateList.Add(dateList[key]);
                }
                //latestDate = dateList.Max();
            }
            else
            {
                latestDate = DateTime.Now;
            }


            foreach (var goal in goalList)
            {
                foreach (var skill in skills)
                {
                    foreach (var date in latestDateList)
                    {
                        if (goal.SkillName == skill.Skill && goal.SkillGoal != -1 && goal.Date == date)
                        {
                            if (!skillList.Contains(skill))
                            {
                                skillList.Add(skill);
                            }
                        }
                    }
                }
            }
            int dictKey = 0;
            TempData.Keep();
            if (skillList.Count() != 0)
            {
                foreach (var skill in skillList)
                {
                    tempList.Add(dictKey, skill.Skill);
                    dictKey++;
                }
                tempModel.SkillList = tempList;
                //model.Add(tempModel);
                return View(tempModel);
            }
            else
            {
                TempData["ActionResult"] = "Can't add skills without adding goals first!";
                return RedirectToAction(nameof(ListByDate), "UserSkills", new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SkillList, SkillLevel, SkillCount")] UserSkillsWithSkillVM userSkills)
        {
            var model = new List<UserSkills>();
            int userId = Convert.ToInt32(TempData["UserId"]);
            TempData.Keep();

            //Date is declared here so that it's guaranteed to be the same for all skills.
            DateTime date = DateTime.Now;

            //Looping through the entries, adding them to a UserSkills object and adding that to a list.
            for (int i = 0; i < userSkills.SkillList.Count(); i++)
            {
                var tempModel = new UserSkills
                {
                    SkillLevel = userSkills.SkillLevel[i],
                    SkillName = userSkills.SkillList[i],
                    UserID = userId,
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
            return RedirectToAction(nameof(ListByDate), "UserSkills", new { id = userId });
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
                return RedirectToAction("SkillList", "UserSkills", new { id = userSkills.UserID, name = userSkills.Date.ToString("dd/MM/yyyy+HH/mm") });
            }
            return View(userSkills);
        }

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

        //GET
        [HttpGet]
        public async Task<IActionResult> EditForm(string date, string name, int id)
        {
            //Try to get the date to be shown in the view
            try
            {
                ViewBag.Date = date;
            }
            catch
            {
                ViewBag.Date = "INVALID_DATE";
            }

            //Getting the correct entries by date and userId
            var userSkills = _context.UserSkills.ToList();
            var viewModel = new EditFormVM();
            var model = new List<UserSkills>();
            foreach (var skill in userSkills)
            {
                if (skill.Date.ToString("dd/MM/yyyy+HH/mm") == name && skill.UserID == id)
                {
                    model.Add(skill);
                }
            }
            viewModel.UserSkills = model;
            return View(viewModel);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForm(int id, [Bind("UserSkills")] EditFormVM UserSkillsVM)
        {
            //Looping through the entries, and updating them
            foreach (var skill in UserSkillsVM.UserSkills)
            {
                _context.Update(skill);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListByDate), "UserSkills", new { id = id });
        }
        

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userSkills = await _context.UserSkills.FindAsync(id);            
            _context.UserSkills.Remove(userSkills);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(SkillList), "UserSkills", new { id = userSkills.UserID, name = userSkills.Date.ToString("dd/MM/yyyy+HH/mm") });
        }

        public async Task<IActionResult> DeleteForm(string name, int? id)
        {
            var userSkillList = _context.UserSkills.ToList();

            foreach (var userSkill in userSkillList)
            {
                if (userSkill.Date.ToString("dd/MM/yyyy+HH/mm") == name && userSkill.UserID == id)
                {
                    _context.Remove(userSkill);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListByDate), "UserSkills", new { id = id });
        }

        private bool UserSkillsExists(int id)
        {
            return _context.UserSkills.Any(e => e.Id == id);
        }

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
