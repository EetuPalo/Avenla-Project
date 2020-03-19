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

        public async Task <IActionResult> ListByDate(int? id, string searchString, int? month, int? year)
        {
            //NULL Handling
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
            if (!month.HasValue)
            {
                month = DateTime.Now.Month;
            }
            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }
            //

            uId = (int)id;
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserName"] = tempUser.UserName;
            ViewBag.UserNames = tempUser.FirstName + " " + tempUser.LastName;

            var model = new List<DateListVM>();
            var tempDate = new List<string>();
            List<DataPoint> dataPoints = new List<DataPoint>();
            List<string> dates = new List<string>();
            List<string> skillnames = new List<string>();
            int i = 0;

            //This is for the search
            var userSkills = from c in _context.UserSkills select c;
            if (!String.IsNullOrEmpty(searchString))                
            {
                //Reformatting the string
                //searchString = searchString.Replace('/', '.');
                var splitDate = searchString.Split('/');

                foreach (var item in userSkills.Where(x => (x.Date.Day == Convert.ToInt32(splitDate[1])) && (x.Date.Month == Convert.ToInt32(splitDate[0])) && (x.Date.Year == Convert.ToInt32(splitDate[2])) && (x.UserID == id)))
                {
                    month = item.Date.Month;
                    year = item.Date.Year;
                    if (!tempDate.Contains(item.Date.ToString()))
                    {
                        i++;
                        var tempModel = new DateListVM
                        {
                            Date = item.Date,
                            AdminEval = item.AdminEval,
                            TempDate = item.Date.ToString("dd/MM/yyyy+HH/mm"),
                            Id = (int)id
                        };
                        model.Add(tempModel);
                    }
                    tempDate.Add(item.Date.ToString());
                    if (item.Date.Month == month && item.Date.Year == year)
                    {
                        skillnames.Add(item.SkillName);
                        dates.Add(item.Date.ToString("dd.MM.yyyy.HH.mm.ss"));
                        dataPoints.Add(new DataPoint(item.Date.Day, item.SkillLevel));
                    }
                }
            }
            else
            {
                foreach (var item in _context.UserSkills.Where(x => x.UserID == id))
                {
                    if (!tempDate.Contains(item.Date.ToString()))
                    {
                        i++;
                        var tempModel = new DateListVM
                        {
                            Date = item.Date,
                            AdminEval = item.AdminEval,
                            TempDate = item.Date.ToString("dd/MM/yyyy+HH/mm"),
                            Id = (int)id
                        };
                        model.Add(tempModel);

                    }
                    tempDate.Add(item.Date.ToString());

                    if (item.Date.Month == month && item.Date.Year == year)
                    {
                        skillnames.Add(item.SkillName);
                        dates.Add(item.Date.ToString("dd.MM.yyyy.HH.mm.ss"));
                        dataPoints.Add(new DataPoint(item.Date.Day, item.SkillLevel));
                    }
                }
            }
           
            ViewBag.DataPoint = dataPoints.ToArray();
            ViewBag.Dates = dates.ToArray();
            ViewBag.names = skillnames.ToArray();
            return View(model);
        }

        [HttpGet]
#nullable enable
        public async Task<IActionResult> SkillList(string? name, int? id)
        {
            if (id == null)
            {
                id = Convert.ToInt32(UserMgr.GetUserId(User));
            }

            var model = new List<UserSkillsVM>();
            TempData.Keep();
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            string userName = tempUser.UserName;
            TempData["UserName"] = userName;
            TempData["UserId"] = id;
            string tempName = "DATE_NOT_FOUND";

            //Getting the skillgoal info for user group
            var groupList = gMemContext.GroupMembers.Where(x => x.UserID == id).ToList();
            var goalList = new List<SkillGoals>();
            var dateList = new Dictionary<string, DateTime>();

            try
            {                
                foreach (var group in groupList)
                {
                    goalList = goalContext.SkillGoals.Where(x => x.GroupName == group.GroupName).ToList();
                }
            }
            catch
            {
                Console.WriteLine("ERROR: An exception occured in listing goals.");
            }          
            
            //If the skilllist is accessed directly from AppUser Index, the latest entries are automatically shown.
            if (name == "latest")
            {
                try
                {
                    var tempDateList = new List<DateTime>();
                    foreach (var skill in _context.UserSkills.Where(x => x.UserID == id))
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
                catch
                {
                    Console.WriteLine("ERROR: An exception occured when fetching latest entries.");
                }
            }
            //This fetches the correct entries and displays them in a list
            if (_context.UserSkills != null)
            {
                foreach (var skill in _context.UserSkills.Where(x => x.UserID == id))
                {
                    var date1 = skill.Date.ToString("dd/MM/yyyy+HH/mm");
                    var date2 = name;
                    var skillGoal = 0;

                    if (date1 == date2)
                    {
                        tempName = skill.Date.ToString("dd.MM.yyyy HH.mm");
                        TempData["Date"] = tempName;

                        var usrSkill = new UserSkillsVM
                        {
                            Id = Convert.ToInt32(skill.Id),
                            UserID = skill.UserID,
                            UserName = userName,
                            SkillName = skill.SkillName,
                            SkillLevel = skill.SkillLevel,
                            Date = skill.Date.ToString("dd/MM/yyyy H:mm"),
                            AdminEval = skill.AdminEval
                        };

                        foreach (var goal in goalList.Where(x => x.SkillGoal > -1))
                        {
                            foreach (var group in groupList.Where(x => x.GroupName == goal.GroupName))
                            {
                                if (dateList.ContainsKey(group.GroupName))
                                {
                                    dateList[group.GroupName] = goal.Date;
                                }
                                else if (!dateList.ContainsKey(group.GroupName))
                                {
                                    dateList.Add(group.GroupName, goal.Date);
                                }
                            }
                        }
                        foreach (var date in dateList.Values)
                        {
                            foreach (var goal in goalList.Where(x => (x.Date == date) && (x.SkillName == skill.SkillName) && (x.SkillGoal > skillGoal)))
                            {
                                skillGoal = goal.SkillGoal;
                            }
                        }
                        usrSkill.SkillGoal = skillGoal;
                        if (usrSkill != null)
                        {
                            model.Add(usrSkill);
                        }
                    }
                }

                //Getting some extra info from the data if it's possible
                try
                {
                    var levelList = new Dictionary<string, int>();
                    foreach (var skill in model)
                    {
                        levelList.Add(skill.SkillName, skill.SkillLevel);
                    }
                    TempData["MinSkillLabel"] = levelList.FirstOrDefault(x => x.Value == levelList.Values.Min()).Key;
                    TempData["MaxSkillLabel"] = levelList.FirstOrDefault(x => x.Value == levelList.Values.Max()).Key;

                    TempData["MinSkillVal"] = levelList.FirstOrDefault(x => x.Value == levelList.Values.Min()).Value;
                    TempData["MaxSkillVal"] = levelList.FirstOrDefault(x => x.Value == levelList.Values.Max()).Value;

                    double avrg = levelList.Values.Average();
                    TempData["AverageScore"] = String.Format("{0:0.00}", avrg);
                }
                catch
                {
                    Console.WriteLine("ERROR");
                    TempData["MinSkillLabel"] = 0;
                    TempData["MaxSkillLabel"] = 0;
                    TempData["MinSkillVal"] = 0;
                    TempData["MaxSkillVal"] = 0;
                    TempData["AverageScore"] = 0;
                }
                //------
                TempData.Keep();
                return View(model);
            }
            else
            {
                return View();
            }           
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
        public IActionResult Create(int id)
        {
            TempData["UserId"] = id;
            var model = new UserSkillsWithSkillVM();
            var tempList = new Dictionary<int, string>();

            //Code here for creating the form based on the skillgoals (if -1, not part of the form)
            var groupList = new List<Group>();
            var skillList = new List<Skills>();
            var goalList = new List<SkillGoals>();
            var dateList = new Dictionary<string, DateTime>();

            foreach (var member in gMemContext.GroupMembers.Where(x => x.UserID == id))
            {
                foreach (var group in groupContext.Group.Where(x => x.name == member.GroupName))
            {
                    foreach (var goal in goalContext.SkillGoals.Where(x => x.GroupName == group.name))
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
            }
            else
            {
                latestDate = DateTime.Now;
            }

            foreach (var goal in goalList.Where(x => x.SkillGoal != -1))
            {
                foreach (var skill in skillContext.Skills.Where(x => x.Skill == goal.SkillName))
                {
                    foreach (var date in latestDateList.Where(x => x == goal.Date))
                    {
                        if (!skillList.Contains(skill))
                        {
                            skillList.Add(skill);
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
                model.SkillList = tempList;
                return View(model);
            }
            else
            {
                TempData["ActionResult"] = "Add the user to a group first.";
                TempData["Status"] = "failed";
                return RedirectToAction(nameof(SkillList), "UserSkills", new { id = id, name = "latest" });
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
            return RedirectToAction(nameof(SkillList), "UserSkills", new { id = userId, name = "latest" });
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
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult EditForm(string date, string name, int id)
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
            var viewModel = new EditFormVM();
            var model = new List<UserSkills>();
            foreach (var skill in _context.UserSkills.Where(x => x.UserID == id))
            {
                if (skill.Date.ToString("dd/MM/yyyy+HH/mm") == name)
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

        [Authorize(Roles = "Admin")]
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
