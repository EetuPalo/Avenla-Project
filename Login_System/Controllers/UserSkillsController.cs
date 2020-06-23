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
    [Authorize(Roles = "User, Admin, Superadmin")]
    public class UserSkillsController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        int uId;

        //These will be set in the index, and be used by other controller methods.
        public int userId;
        public string userName;

        public UserSkillsController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
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

            //Month and year are needed for the graph
            if (!month.HasValue)
            {
                month = DateTime.Now.Month;
            }

            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }            

            //Some data that will be shown in the view
            uId = (int)id;
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserName"] = tempUser.UserName;
            ViewBag.UserNames = tempUser.FirstName + " " + tempUser.LastName;

            var model = new List<DateListVM>();
            var tempDate = new List<string>();

            List<List<DataPoint>> datapointsPerSkill= new List<List<DataPoint>>();
            List<DataPoint> dataPoints = new List<DataPoint>();
            List<string> dates = new List<string>();
            List<string> skillnames = new List<string>();
            int i = 0;

            //This is for the search
            var userSkills = from c in _context.UserSkills select c;
            TempData["SearchValue"] = null;

            //SEARCH
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
                TempData["SearchValue"] = searchString;
            }
            //NO SEARCH
            else
            {
                foreach(var skillName in _context.Skills)
                {

                
                //Getting all items of the specific user
                    foreach (var item in _context.UserSkills.Where(x => x.UserID == id && x.SkillName == skillName.Skill))
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
                            if (!skillnames.Contains(item.SkillName))
                            {
                                skillnames.Add(item.SkillName);
                            }
                            if (dates.Contains(item.Date.ToString("dd.MM.yyyy.HH.mm.ss")))
                            {
                                dates.Add(item.Date.ToString("dd.MM.yyyy.HH.mm.ss"));
                            }
                           
                            dataPoints.Add(new DataPoint(item.Date.Day, item.SkillLevel));
                        }
                        
                    }
                    if (dataPoints.Count >0) { 
                    var x = dataPoints.ToList();
                    datapointsPerSkill.Add(x) ;
                    dataPoints.Clear();
                    }
                }
            }

            //Graph stuff
            //ViewBag.DataPoint = dataPoints.ToArray();
            ViewBag.DataPoint = datapointsPerSkill;
            ViewBag.Dates = dates.ToArray();
            ViewBag.names = skillnames.ToArray();

            Console.WriteLine(ViewBag.names[0]);
            return View(model);
        }

        [HttpGet]
#nullable enable
        public async Task<IActionResult> SkillList(string? name, int? id)
        {
            //In case id is null, the currently logged in user is shown (this shouldn't happen though)
            if (id == null)
            {
                id = Convert.ToInt32(UserMgr.GetUserId(User));
            }

            //Initializing some stuff
            var model = new List<UserSkillsVM>();
            TempData.Keep();
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            string userName = tempUser.UserName;
            TempData["UserName"] = userName;
            TempData["UserId"] = id;
            string tempName = "DATE_NOT_FOUND";

            //Getting the skillgoal info for user's group(s)
            var groupList = _context.GroupMembers.Where(x => x.UserID == id).ToList();
            var goalList = new List<SkillGoals>();
            var dateList = new Dictionary<string, DateTime>();

            try
            {                
                foreach (var group in groupList)
                {
                    goalList = _context.SkillGoals.Where(x => x.GroupName == group.GroupName).ToList();
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
                //Looping through userskills of the current user
                foreach (var skill in _context.UserSkills.Where(x => x.UserID == id))
                {
                    var date1 = skill.Date.ToString("dd/MM/yyyy+HH/mm");
                    var date2 = name;
                    var skillGoal = 0;
                    //Getting only the ones with the date that has been selected by the user (or the latest date)
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

                        //Getting goals of the group 
                        foreach (var goal in goalList.Where(x => x.SkillGoal > -1))
                        {
                            //Narrowing down to only the goals of the right group(s)
                            foreach (var group in groupList.Where(x => x.GroupName == goal.GroupName))
                            {
                                //Adding goals to a list with goal and date
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
                        //Looping through the datelist that was just populated
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
                //It's not possible in all cases (for example if there's not enough data), hence the try-catch
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
            var tempList = new List<Skills>();

            //Code here for creating the form based on the skillgoals (if -1, not part of the form)
            var groupList = new List<Group>();
            var skillList = new List<Skills>();
            var goalList = new List<SkillGoals>();
            //var dateList = new Dictionary<string, DateTime>();

            //Going through groupmember info of the specified user
            foreach (var member in _context.GroupMembers.Where(x => x.UserID == id))
            {
                //Going through all groups the user is part of
                foreach (var group in _context.Group.Where(x => x.name == member.GroupName))
                {
                    //Going through all goals of the groups and adding them to a dictionary with both group name and date
                    foreach (var goal in _context.SkillGoals.Where(x => x.Groupid == group.id))
                    {
                        /*DateTime value = goal.Date;

                        //If group is already in the dict, only the date is replaced
                        if (dateList.ContainsKey(group.name))
                        {
                            dateList[group.name] = value;
                        }

                        //Else it creates a new entry
                        else
                        {
                            dateList.Add(group.name, value);
                        }*/
                        goalList.Add(goal);
                    }
                }
            }
            
            DateTime latestDate = DateTime.Now;
            var latestDateList = new List<DateTime>();

            /*if (dateList.Count() != 0)
            {
                foreach (var key in dateList.Keys)
                {
                    latestDateList.Add(dateList[key]);
                }
            }

            else
            {
                latestDate = DateTime.Now;
            }*/

            //Going through all goals that are not -1
            //This dictates what skills are in the form
            // REMEMBER TO CHANGE BACK TO -1
            foreach (var goal in goalList.Where(x => x.SkillGoal != -2))
            {
                //Getting the skills
                foreach (var skill in _context.Skills.Where(x => x.Skill == goal.SkillName))
                {
                    //Making sure the latest goal dates are used
                
                        //Skill can be added only once to the list
                        //This avoids duplicates
                        if (!skillList.Contains(skill))
                        {
                            skillList.Add(skill);
                        }
                    
                }
            }
            TempData.Keep();

            //This is only executed if there are skills that need to be in the form
            if (skillList.Count() != 0)
            {
                //The list is populated
                foreach (var skill in skillList)
                {
                    tempList.Add(skill);
                
                }
                model.SkillList = tempList;

                return View(model);
            }
            else
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_AddToGroup;
                TempData["Status"] = Resources.ActionMessages.ActionResult_GeneralFail;

                return RedirectToAction(nameof(SkillList), "UserSkills", new { id = id, name = "latest" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SkillList, SkillLevel, SkillCount")]  List<Skills>SkillList ,int[] Skillid, int[] SkillLevel )
        {
            var model = new List<UserSkills>();
            int userId = Convert.ToInt32(TempData["UserId"]);
            TempData.Keep();
            int i= 0;
            //Date is declared here so that it's guaranteed to be the same for all skills.
            DateTime date = DateTime.Now;

             
            foreach(var id in Skillid)
            {
                var skill = await _context.Skills.FirstOrDefaultAsync(x=> x.Id == id);
                var newUserSkill = new UserSkills
                {
                    Skillid = id,
                    SkillName = skill.Skill,
                    UserID = userId,
                    SkillLevel = SkillLevel[i],
                    Date = DateTime.Now
                };
                if (User.IsInRole("Admin"))
                {
                    newUserSkill.AdminEval = "Admin Evaluation";
                }

                else
                {
                    newUserSkill.AdminEval = "Self Assessment";
                }
                model.Add(newUserSkill);
            }

            /*var tempModel = new UserSkills
                {
                    SkillLevel = SkillLevel,
                    SkillName = userSkills.SkillList[i],
                    UserID = userId,
                    Id = null,
                    Date = date,
                    //Skillid = 
                    
                };

                //info of who made the evaluation
                if (User.IsInRole("Admin"))
                {
                    tempModel.AdminEval = "Admin Evaluation";
                }

                else
                {
                    tempModel.AdminEval = "Self Assessment";
                }

                model.Add(tempModel);
            */

            //Adding entries to database and saving
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
                ViewBag.Date = Resources.ActionMessages.ViewBag_InvalidDate;
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
            foreach (var userSkill in _context.UserSkills.Where(x => x.UserID == id).ToList())
            {
                if (userSkill.Date.ToString("dd/MM/yyyy+HH/mm") == name)
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

            foreach (var skill in _context.UserSkills.Where(x => (x.SkillName == skillName) && (x.UserID == userId)))
            {
                entryCount++;
            }
            TempData.Keep();

            return entryCount;
        }

        public DateTime GetLatest (string skillName)
        {
            DateTime latestDate;
            List<DateTime> allDates = new List<DateTime>();
            int userId = Convert.ToInt32(TempData["UserId"]);

            foreach (var skill in _context.UserSkills.Where(x => (x.SkillName == skillName) && (x.UserID == userId)))
            {
                allDates.Add(skill.Date);
            }

            latestDate = allDates.Max();
            TempData.Keep();

            return latestDate;
        }

        public int GetLatestEval (string skillName, DateTime latestDate)
        {
            int latestEval = 0;
            int userId = Convert.ToInt32(TempData["UserId"]);

            foreach (var skill in _context.UserSkills.Where(x => (x.SkillName == skillName) && (x.UserID == userId) && (x.Date == latestDate)))
            {
                latestEval = skill.SkillLevel;
            }

            TempData.Keep();

            return latestEval;
        }
    }
}
