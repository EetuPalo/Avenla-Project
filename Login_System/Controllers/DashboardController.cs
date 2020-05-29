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
            
            var user = await UserMgr.GetUserAsync(HttpContext.User);
            ViewBag.CurrentCompany = user.Company;            
            ViewBag.CurrentUserLastName = user.FirstName;
            ViewBag.CurrentUserFirstName = user.LastName;
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

            //Populating the VM with group, course, and certificate info
            var skillList = new List<UserSkills>();
            var courseList = new List<SkillCourseMember>();
            var certificateList = new List<UserCertificate>();

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

            // Courses
            foreach (var courseMember in _context.SkillCourseMembers.Where(x => x.UserID == id))
            {
                courseList.Add(courseMember);
            }

            // Certificates
            foreach (var userCertificate in _context.UserCertificates.Where(x => x.UserID == id))
            {
                certificateList.Add(userCertificate);
            }

            model.UserSkills = skillList;
            model.UserCourses = courseList;
            model.UserCertificates = certificateList;

            return View(model);
        }
    }
}