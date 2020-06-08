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

namespace Login_System.Controllers
{
    [Authorize(Roles = "User, Admin, Superadmin")]
    public class DashboardController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

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
            ViewBag.CurrentCompany = user.Company;            
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

            var groupList = _context.GroupMembers.Where(x => x.UserID == id).ToList();


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
                goalList = _context.SkillGoals.Where(x => x.GroupName == group.GroupName && x.SkillGoal >-1).ToList();
               
                foreach (var groupSkillGoals in goalList.Where(x=> x.GroupName == group.GroupName))
                {
                    var goalDate = from t in _context.SkillGoals
                                   where t.GroupName == groupSkillGoals.GroupName
                                   group t by t.SkillName into g
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

            model.UserGroups = groupList;
            model.UserSkills = skillList;
            model.UserCourses = courseList.OrderBy(x=>x.Status).ToList();
            model.UserCertificates = certificateList;
            model.UserGoals = allGoals;
            model.Lessons = upcomingLessonsList.OrderBy(x=>x.Date).Take(5).ToList();
            model.PastLessons = pastLessonsList.OrderByDescending(x => x.Date).Take(5).ToList();
            


            return View(model);
        }

    }
}