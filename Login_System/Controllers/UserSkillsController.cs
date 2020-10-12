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

        

            //Some data that will be shown in the view
            uId = (int)id;
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserName"] = tempUser.UserName;
            ViewBag.UserNames = tempUser.FirstName + " " + tempUser.LastName;

            var model = new List<DateListVM>();
            var tempDate = new List<string>();

            List<string> uniqueMonths = new List<string>();

            var skillPoints = new List<List<SkillPoint>>();
            var datapoint = new List<SkillPoint>();
            var dataPoints = new List<SkillPoint>();

            //this list is to hel get the latest evaluation on daily basis
            List<string> evaluationDates = new List<string>();

            List<List<DataPoint>> datapointsPerSkill= new List<List<DataPoint>>();
           
            List<string> dates = new List<string>();
            List<int> years = new List<int>();
            List<string> skillnames = new List<string>();
            int i = 0;

            //This is for the search
            var userSkills = from c in _context.UserSkills select c;
            TempData["SearchValue"] = null;

           
            
            //SEARCH
            if (month != null || year != null)
            {
                // if month is given but not year, user mostlikely wants to search the current years month.
                if (month != null && year == null)
                {
                    year = DateTime.Now.Year;
                }

                //check if given value is null, if is, dont take it into account when fetching from table.
                foreach(var skill in _context.Skills)
                {
                    foreach (var item in userSkills.Where(x => ((month != null) ? (x.Date.Month == month) : (x.Date.Month != null)) && x.Date.Year == year && x.UserID == id && x.SkillId == skill.Id).OrderByDescending(x=> x.Date))
                    {
                        var itemMonth = item.Date.Month;
                        var itemYear = item.Date.Year;
                        if (!tempDate.Contains(item.Date.ToString()))
                        {
                            i++;
                            var tempModel = new DateListVM
                            {
                                Date = item.Date,
                                AdminEval = item.AdminEval,
                                TempDate = item.Date.ToString("dd/MM/yyyy"),
                                Id = (int)id
                            };
                            model.Add(tempModel);
                        }
                        tempDate.Add(item.Date.ToString());
                        if (!skillnames.Contains(skill.Skill))
                        {
                            skillnames.Add(skill.Skill);
                        }
                        if ((month != null)?item.Date.Month == month : item.Date.Year == year && item.Date.Year == year)
                        {
                           
                            dates.Add(item.Date.ToString("dd.MM.yyyy"));
                            datapoint.Add(new SkillPoint(item.Date.ToString("dd.MM.yyyy"), item.SkillLevel));
                        }
                    }
                    if (datapoint.Count > 0)
                    {
                       
                        skillPoints.Add(datapoint.ToList());
                        datapoint.Clear();
                    }
                }
             
                TempData["SearchValue"] = searchString; 
               
            }
            

            //NO SEARCH
            else
            {
          
                foreach(var skillName in _context.Skills)
                {

                    evaluationDates.Clear();
                //Getting all items of the specific user
                    foreach (var item in _context.UserSkills.Where(x => x.UserID == id && x.SkillName == skillName.Skill).OrderByDescending(x=> x.Date))
                    {
                        if (!evaluationDates.Contains(item.Date.ToString("dd.MM.yyyy")))
                        {
                            evaluationDates.Add(item.Date.ToString("dd.MM.yyyy"));
                            if (!tempDate.Contains(item.Date.ToString()))
                            {
                                i++;

                                var tempModel = new DateListVM
                                {
                                    Date = item.Date,
                                    AdminEval = item.AdminEval,
                                    TempDate = item.Date.ToString("dd.MM.yyyy"),
                                    Id = (int)id
                                };
                                model.Add(tempModel);
                            }
                            tempDate.Add(item.Date.ToString());


                            if (!skillnames.Contains(item.SkillName))
                            {
                                skillnames.Add(item.SkillName);
                            }
                            if (!dates.Contains(item.Date.ToString("dd.MM.yyyy")))
                            {
                                dates.Add(item.Date.ToString("dd.MM.yyyy"));
                            }

                            if (!uniqueMonths.Contains(item.Date.Month.ToString() + " " + item.Date.Month.ToString()))
                            {
                                uniqueMonths.Add(item.Date.Month.ToString() + " " + item.Date.Month.ToString());
                            }
                            datapoint.Add(new SkillPoint(item.Date.ToString("dd.MM.yyyy"), item.SkillLevel));

                        }
                    }
                    if (datapoint.Count > 0)
                    {
                        skillPoints.Add(datapoint.ToList());
                        datapoint.Clear();
                    }
                }
            
            }
            years = (from Year in _context.UserSkills
                     select Year.Date.Year)
                      .Distinct().ToList();
            //Graph stuff
            ViewBag.DataPoint = skillPoints;
            ViewBag.Dates = dates.ToArray();
            ViewBag.names = skillnames.ToArray();
            ViewBag.numberOfMonths = uniqueMonths.ToArray();
            ViewBag.years = years.ToArray();
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
            TempData["FullName"] = tempUser.FirstName + " " + tempUser.LastName;
            string tempName = "DATE_NOT_FOUND";
            var skillIdList = new List<int>();           

            //Getting the skillgoal info for user's group(s)
            var groupList = _context.GroupMembers.Where(x => x.UserID == id).ToList();
            var goalList = new List<SkillGoals>();
            var tempGoalList = new List<SkillGoals>();
            var dateList = new Dictionary<string, DateTime>();
            /*TempData["Date"] = _context.UserSkills.OrderByDescending(x=> x.Date).FirstOrDefault().Date.ToString("dd.MM.yyyy HH.mm");*/    
            try
            {                
                foreach (var group in groupList)
                {
                    tempGoalList = _context.SkillGoals.Where(x => x.GroupId == group.GroupID).ToList();
                    foreach(var skillid in tempGoalList)
                    {
                        //get latest goals of a skill from a group user belongs to.
                        var latestGoal = tempGoalList.OrderByDescending(x=> x.Date).First(x => x.SkillId == skillid.SkillId);
                        if (!goalList.Contains(latestGoal))
                        {
                            goalList.Add(latestGoal);
                        }
                    }
                }
            }

            catch
            {
                Console.WriteLine("ERROR: An exception occured in listing goals.");
            }          
            
            //If the skilllist is accessed directly from AppUser Index, the latest entries are automatically shown.
  
            //This fetches the correct entries and displays them in a list
            if (_context.UserSkills != null)
            {
                //Looping through userskills of the current user
                foreach (var skill in _context.UserSkills.Where(x => x.UserID == id).OrderByDescending(x=> x.Date))
                {
                  
                    
                    if (!skillIdList.Contains(skill.SkillId))
                    {
                        skillIdList.Add(skill.SkillId);

                        //Getting only the ones with the date that has been selected by the user (or the latest date)
                        var maxGoal = goalList.Where(x => x.SkillId == skill.SkillId).Max(x=> (int?)x.SkillGoal);



                            var usrSkill = new UserSkillsVM
                        {
                            Id = Convert.ToInt32(skill.Id),
                            UserID = skill.UserID,
                            UserName = userName,
                            SkillName = skill.SkillName,
                            SkillLevel = skill.SkillLevel,
                            Date = skill.Date.ToString("dd/MM/yyyy H:mm"),
                            AdminEval = skill.AdminEval,
                            SkillGoal = (maxGoal != null && maxGoal != -1)? maxGoal : 0
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
            List<int> tempSkill = new List<int>();
            TempData["UserId"] = id;
            var model = new UserSkillsWithSkillVM();
            var tempList = new List<Skills>();

            List<UserSkills> usrSkl = new List<UserSkills>();
            //Code here for creating the form based on the skillgoals (if -1, not part of the form)
            var groupList = new List<Group>();
            var skillList = new List<Skills>();
            var goalList = new List<SkillGoals>();
            //var dateList = new Dictionary<string, DateTime>();

            //Going through groupmember info of the specified user
            foreach (var member in _context.GroupMembers.Where(x => x.UserID == id))
            {
                //Going through all groups the user is part of
                foreach (var group in _context.Group.Where(x => x.id == member.GroupID))
                {
                    //Going through all goals of the groups and adding them to a list
                    foreach (var goal in _context.SkillGoals.Where(x => x.GroupId == group.id))
                    {
                        goalList.Add(goal);
                    }
                }
            }
            
            DateTime latestDate = DateTime.Now;
            var latestDateList = new List<DateTime>();

  
            //Going through all goals that are not -1
            //This dictates what skills are in the form
            // REMEMBER TO CHANGE BACK TO -1
            foreach (var goal in goalList)
            {
                //Getting the skills
                 foreach (var skill in _context.Skills.Where(x => x.Id == goal.SkillId))
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
                    foreach (var userSkill in _context.UserSkills.OrderByDescending(x=> x.Date).Where(x=> x.SkillId == skill.Id && x.UserID == id))
                    {
                        if (!usrSkl.Exists(x=> x.SkillName == skill.Skill))
                        {
                            usrSkl.Add(userSkill);
                        }
                      
                    }
                
                }
                model.SkillList = skillList;
                model.SkillLevel = tempSkill;
                model.UserSkill = usrSkl;

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
        public async Task<IActionResult> Create([Bind("SkillList, SkillLevel, SkillCount")]  List<Skills>SkillList ,int[] Skillid, int[] Skilllevel )
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
                    SkillId = id,
                    SkillName = skill.Skill,
                    SkillLevel = Skilllevel[i],
                    UserID = userId,
                    Date = date
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
                i++;
            }


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

        public class SkillPoint
        {
           public int y { get; set; }
           public string x { get; set; }

            public SkillPoint(string d, int s)
            {
                
                y = s;
                x = d;
            }
        }
    }
}
