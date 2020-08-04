using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Login_System.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using ASPNET_MVC_Samples.Models;

namespace Login_System.Controllers
{
    [Authorize(Roles = "User, Admin, Superadmin")]
    public class DashboardController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        int uId;

        public int userId;

        public DashboardController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }
        public async Task<IActionResult> Index(int? id)
        {
            var model = new DashboardVM();
            DateTime localdate = DateTime.Now;            

            var user = await UserMgr.GetUserAsync(HttpContext.User);
            ViewBag.CurrentCompany = user.CompanyName;            
            ViewBag.CurrentUserFirstName = user.FirstName;
            ViewBag.CurrentUserLastName = user.LastName;
            ViewBag.CurrentUserEmail = user.Email;
            ViewBag.CurrentUserPhone = user.PhoneNumber;

            if (id == null)
            {
                id = Convert.ToInt32(UserMgr.GetUserId(User));
            }
            //Finding the correct user
            var appUser = await UserMgr.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserId"] = id;

            //Populating the VM with all needed info
            var skillList = new List<UserSkills>();
            var courseList = new List<SkillCourseMember>();
            var certificateList = new List<UserCertificate>();
            var goalList = new List<SkillGoals>();
            var userGroupList = new List<GroupMember>();
            var allGoals = new List<SkillGoals>();
            var lessonList = new List<Lesson>();
            var upcomingLessonsList = new List<Lesson>();
            var pastLessonsList = new List<Lesson>();
            var companyDescList = new List<string>();

            var groupList = _context.GroupMembers.Where(x => x.UserID == id).ToList();

            foreach (var company in _context.Company.Where(x=> x.Id == user.Company))
            {
                companyDescList.Add(company.Description);
            }

            // Skills
            foreach (var skill in _context.UserSkills.Where(x => x.UserID == id))
            {
                var skillQueryDate = from t in _context.UserSkills
                                    group t by t.UserID into g
                                    select new { UserID = g.Key, Date = g.Max(t => t.Date) };

                foreach (var it in skillQueryDate.Where(x => x.Date == skill.Date))
                {
                    if (!skillList.Contains(skill))
                    {
                        skillList.Add(skill);
                    }
                }
            }

            // Skillgoals
            foreach (var group in groupList)
            {
                var updatedGoalList = new List<SkillGoals>();
                goalList = _context.SkillGoals.Where(x => x.GroupId == group.GroupID && x.SkillGoal >-1).ToList();
               
                foreach (var groupSkillGoals in goalList.Where(x=> x.GroupId == group.GroupID))
                {
                    var goalDate = from t in _context.SkillGoals
                                   where t.GroupId == groupSkillGoals.GroupId
                                   group t by t.SkillId into g
                                   select new { SkillName = g.Key, Date = g.Max(t => t.Date) };
                    foreach(var goaldates in goalDate.Where(x=> x.Date == groupSkillGoals.Date))
                    {
                        if (!updatedGoalList.Contains(groupSkillGoals))
                        {
                            updatedGoalList.Add(groupSkillGoals);
                        }
                    }
                    
                }
                allGoals.AddRange(updatedGoalList);
            }

            // Populating user groups
            foreach (var group in groupList)
            {
                model.GroupsDD.Add(new SelectListItem() { Text = group.GroupName, Value = group.GroupName });
            }

            // Courses
            foreach (var courseMember in _context.SkillCourseMembers.Where(x => x.UserID == id))
            {
                courseList.Add(courseMember);
            }

            // Lessons
            foreach (var userLessons in _context.LessonUsers.Where(x => x.MemberID == id).ToList())
            {
                foreach(var lessons in _context.Lessons.Where(x=> x.Id == userLessons.LessonID))
                {
                    if (localdate <= lessons.Date) 
                    {
                        upcomingLessonsList.Add(lessons);
                    }
                    if (localdate > lessons.Date)
                    {
                        pastLessonsList.Add(lessons);
                    }
                    //lessonList.Add(lessons);
                }
            }            

            // Certificates
            foreach (var userCertificate in _context.UserCertificates.Where(x => x.UserID == id))
            {
                certificateList.Add(userCertificate);
            }
            //ViewBag.CompanyDesc = company.Description;
            model.UserGroups = groupList;
            model.UserSkills = skillList;
            model.UserCourses = courseList.OrderBy(x=>x.Status).ToList();
            model.UserCertificates = certificateList;
            model.UserGoals = allGoals;
            model.Lessons = upcomingLessonsList.OrderBy(x=>x.Date).Take(5).ToList();
            model.PastLessons = pastLessonsList.OrderByDescending(x => x.Date).Take(5).ToList();
            model.CompanyDesc = companyDescList;

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
            TempData["UserName"] = tempUser.UserName;
            ViewBag.UserNames = tempUser.FirstName + " " + tempUser.LastName;

            var dateModel = new List<DateListVM>();
            var tempDate = new List<string>();


            var testpoints = new List<List<SkillPoint>>();
            var datapoint = new List<SkillPoint>();
            var dataPoints = new List<SkillPoint>();

            List<List<DataPoint>> datapointsPerSkill = new List<List<DataPoint>>();

            List<string> dates = new List<string>();
            List<string> skillnames = new List<string>();
            int i = 0;

            foreach (var skillName in _context.Skills)
            {
                //Getting all items of the specific user
                foreach (var item in _context.UserSkills.Where(x => x.UserID == id && x.SkillName == skillName.Skill).OrderBy(x => x.Date))
                {
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
                        dateModel.Add(tempModel);
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

                    datapoint.Add(new SkillPoint(item.Date.ToString("dd.MM.yyyy"), item.SkillLevel));
                }
                if (datapoint.Count > 0)
                {

                    testpoints.Add(datapoint.ToList());
                    datapoint.Clear();
                }
            }

            ViewBag.DataPoint = testpoints;
            ViewBag.Dates = dates.ToArray();
            ViewBag.names = skillnames.ToArray();

            return View(model);
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