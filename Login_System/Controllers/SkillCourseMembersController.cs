using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;
using Login_System.ViewModels;

namespace Login_System.Controllers
{
    public class SkillCourseMembersController : Controller
    {
        private readonly SkillCourseMemberDataContext _context;        
        private readonly SkillCourseDataContext _sccontext;
        private readonly UserManager<AppUser> UserMgr;

        public SkillCourseMembersController(SkillCourseMemberDataContext context, UserManager<AppUser> userManager, SkillCourseDataContext groups)
        {
            _context = context;
            UserMgr = userManager;
            _sccontext = groups;
        }

        // GET: SkillCourseMembers
        public async Task<IActionResult> Index(int? id)
        {
            var model = new List<SkillCourseMemberVM>();
            //if (id == null)
            //{
            //    Console.WriteLine("DEBUG: No ID has been passed to the controller. Listing the skills of the currently logged in user.");
            //    id = Convert.ToInt32(UserMgr.GetUserId(User));
            //}

            SkillCourse tempCourse = await _sccontext.Courses.FindAsync(id);
            //for loop to iterate through members, but only show current user for now, later will show all group user partakes in(if several)
            foreach (var member in _context.SkillCourseMembers)
            {
                if (member.CourseID == id)
                {
                    var coursemember = new SkillCourseMemberVM();
                    coursemember.Id = member.Id;
                    coursemember.UserID = member.UserID;
                    AppUser tempUser = await UserMgr.FindByIdAsync(coursemember.UserID.ToString());
                    coursemember.UserName = tempUser.UserName;
                    coursemember.CourseName = tempCourse.CourseName;
                    var user = await _context.SkillCourseMembers.FirstOrDefaultAsync(m => m.UserID == coursemember.UserID && m.CourseID == member.CourseID);
                    coursemember.Status = user.Status;
                    model.Add(coursemember);
                }
            }

            //Information that is useful in other methods that is not always available
            TempData["CourseID"] = id;
            try
            {
                TempData["CourseName"] = tempCourse.CourseName;
            }
            catch (NullReferenceException)
            {
                //line 63 causes NullReference exception but doesn't actually prevent the program from working as intended, so the exception is just ignored
                //someday would need to look into it.
            }

            return View(model);
        }

        // GET: SkillCourseMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillCourseMember = await _context.SkillCourseMembers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (skillCourseMember == null)
            {
                return NotFound();
            }

            return View(skillCourseMember);
        }

        // GET: SkillCourseMembers/Create
        public IActionResult Create(int? id)
        {
            var member = UserMgr.Users.ToList();
            if (id != null)
            {
                var model = new SkillCourseMember();
                {
                    model.Uname = member.Select(x => new SelectListItem
                    {
                        Value = x.UserName,
                        Text = x.UserName
                    });//creating a list of dropdownlist elements                 
                    model.CourseID = (int)id;//assigning CourseID of the current group
                    model.CourseName = TempData["CourseName"] as string;//assigning CourseName that we saved as well
                };
                return View(model);
            }
            else
            {
                id = TempData["CourseID"] as int?;//using CourseID we saved earlier in the Index if the id passed to the method is NULL
                var model = new SkillCourseMember();
                {
                    model.Uname = member.Select(x => new SelectListItem
                    {
                        Value = x.UserName,
                        Text = x.UserName
                    });
                    model.CourseID= (int)id;//assigning CourseID of the current group
                    model.CourseName = TempData["CourseName"] as string;//assigning CourseName that we saved as well
                    TempData.Keep();//so the data is not lost because it's TEMPdata (temporary)
                };

                return View(model);
            }
        }

        // POST: SkillCourseMembers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseID, UserID,UserName, CourseName, Status, CompletionDate")] SkillCourseMember skillCourseMember)
        {
            if (ModelState.IsValid)
            {
                var user = await UserMgr.FindByNameAsync(skillCourseMember.UserName);//creating a temp user through username selected in the view
                skillCourseMember.UserID = user.Id;//assinging UserID of the selected user
                skillCourseMember.CourseID = Convert.ToInt32(TempData["CourseID"]);//the id in the temp data is not int so we convert it
                skillCourseMember.CourseName = TempData["CourseName"] as string;//same as id
                //skillCourseMember.CompletionDate = DateTime.Now;
                skillCourseMember.Status = "In-progress";
                TempData.Keep();//keeping the temp data otherwise, skillCourseMember won't have CourseID and CourseName
                _context.Add(skillCourseMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "skillCourseMembers", new { id = skillCourseMember.CourseID });//redirecting back to the list of group members,
                // without specifying the id, an empty list is shown
            }
            return View(skillCourseMember);
        }

        // GET: SkillCourseMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillCourseMember = await _context.SkillCourseMembers.FindAsync(id);
            if (skillCourseMember == null)
            {
                return NotFound();
            }
            return View(skillCourseMember);
        }

        // POST: SkillCourseMembers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, UserID, UserName, CourseName, CourseID, Status")] SkillCourseMember skillCourseMember)
        {
            if (id != skillCourseMember.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skillCourseMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillCourseMemberExists(skillCourseMember.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(skillCourseMember);
        }

        // GET: SkillCourseMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillCourseMember = await _context.SkillCourseMembers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (skillCourseMember == null)
            {
                return NotFound();
            }

            return View(skillCourseMember);
        }

        // POST: SkillCourseMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skillCourseMember = await _context.SkillCourseMembers.FindAsync(id);
            _context.SkillCourseMembers.Remove(skillCourseMember);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), "SkillCourseMembers", new { id = skillCourseMember.CourseID});//redirecting back to the list of group members after deletion
        }

        private bool SkillCourseMemberExists(int id)
        {
            return _context.SkillCourseMembers.Any(e => e.Id == id);
        }
    }
}
