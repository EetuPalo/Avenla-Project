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
using Microsoft.AspNetCore.Authorization;

namespace Login_System.Controllers
{
    public class SkillCourseMembersController : Controller
    {
        private readonly SkillCourseMemberDataContext _context;        
        private readonly SkillCourseDataContext _sccontext;
        private readonly UserManager<AppUser> UserMgr;
        private readonly LessonDataContext lessonContext;
        private readonly LessonUserDataContext lessonUserContext;

        public SkillCourseMembersController(SkillCourseMemberDataContext context, UserManager<AppUser> userManager, SkillCourseDataContext groups, LessonDataContext lesCon, LessonUserDataContext lesUsrCon)
        {
            _context = context;
            UserMgr = userManager;
            _sccontext = groups;
            lessonContext = lesCon;
            lessonUserContext = lesUsrCon;
        }

        // GET: SkillCourseMembers
#nullable enable
        public async Task<IActionResult> Index(int? id, string? courseName)
        {
            var model = new List<SkillCourseMemberVM>();
            SkillCourse tempCourse = new SkillCourse();

            if (id == null && courseName != null)
            {
                var findCourse = _sccontext.Courses.FirstOrDefault(x => x.CourseName == courseName);
                id = findCourse.id;
            }
            if (id != null)
            {
                tempCourse = await _sccontext.Courses.FindAsync(id);
            }

            //For loop to iterate through members, but only show current user for now, later will show all group user partakes in(if several)
            foreach (var member in _context.SkillCourseMembers.Where(x => x.CourseID == id))
            {
                int counter = 0;
                AppUser tempUser = await UserMgr.FindByIdAsync(member.UserID.ToString());
                var coursemember = new SkillCourseMemberVM
                {
                    Id = member.Id,
                    UserID = member.UserID,
                    CourseGrade = member.CourseGrade,
                    UserName = tempUser.UserName,
                    CourseName = tempCourse.CourseName
                };

                //Setting the new "Days Completed" number based on the amount of lessons user has attended
                foreach (var lesson in lessonContext.Lessons.Where(x => x.CourseID == id))
                {
                    foreach (var lessonUser in lessonUserContext.LessonUsers.Where(x => (x.MemberID == coursemember.UserID) && (x.LessonID == lesson.Id)))
                    {
                        counter++;
                    }
                }
                coursemember.DaysCompleted = counter;
                var user = await _context.SkillCourseMembers.FirstOrDefaultAsync(m => m.UserID == coursemember.UserID && m.CourseID == member.CourseID);
                coursemember.Status = user.Status;
                coursemember.CourseLength = tempCourse.Length;
                model.Add(coursemember);
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

            TempData["CourseMemberCount"] = model.Count();
            TempData["CompletedCount"] = model.Where(x => x.Status == "Completed").Count();
	    TempData["DropoutCount"] = model.Where(x => x.Status == "Dropout").Count();
            return View(model);
        }

        // GET: SkillCourseMembers/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            var model = new List<SkillCourseMemberVM>();
            //for loop to iterate through members, but only show current user for now, later will show all group user partakes in(if several)
            foreach (var member in _context.SkillCourseMembers.Where(x => x.UserID == id))
            {
		        try
		        {
                    var coursemember = new SkillCourseMemberVM
                    {
                        Id = member.Id,
                        UserID = member.UserID
                    };

                    AppUser tempUser = await UserMgr.FindByIdAsync(coursemember.UserID.ToString());
                    coursemember.UserName = tempUser.UserName;

                    var user = await _context.SkillCourseMembers.FirstOrDefaultAsync(m => m.UserID == coursemember.UserID && m.CourseID == member.CourseID);
                    coursemember.CourseName = user.CourseName;
                    coursemember.DaysCompleted = user.DaysCompleted;
                    coursemember.CompletionDate = user.CompletionDate;

                    var lacourse = await _sccontext.Courses.FirstOrDefaultAsync(m => m.id == user.CourseID);
                    coursemember.CourseLength = lacourse.Length;
                    coursemember.Status = user.Status;
		    
                    model.Add(coursemember);
                }
		        catch (NullReferenceException)
		        {
		    
		        }               		
            }
            return View(model);
        }

        // GET: SkillCourseMembers/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create(int? id)
        {
            var members = UserMgr.Users.ToList();            
            if (id != null)
            {
                var model = new SkillCourseMember();
                {
                    model.Uname = members.Select(x => new SelectListItem
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
                    model.Uname = members.Select(x => new SelectListItem
                    {
                        Value = x.UserName,
                        Text = x.UserName
                    });
                    model.CourseID = (int)id;//assigning CourseID of the current group
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("CourseID, UserID,UserName, CourseName, Status, CompletionDate")] SkillCourseMember skillCourseMember)
        {
            if (ModelState.IsValid)
            {
                var user = await UserMgr.FindByNameAsync(skillCourseMember.UserName);//creating a temp user through username selected in the view
                skillCourseMember.UserID = user.Id;//assinging UserID of the selected user
                skillCourseMember.CourseID = Convert.ToInt32(TempData["CourseID"]);//the id in the temp data is not int so we convert it
                skillCourseMember.CourseName = TempData["CourseName"] as string;//same as id                
                skillCourseMember.Status = skillCourseMember.Status;
                TempData.Keep();//keeping the temp data otherwise, skillCourseMember won't have CourseID and CourseName
                _context.Add(skillCourseMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "skillCourseMembers", new { id = skillCourseMember.CourseID });//redirecting back to the list of group members,
                // without specifying the id, an empty list is shown
            }
            return View(skillCourseMember);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AddUsers(int id)
        {
            var model = new List<SkillCourseMember>();
            var userList = _context.SkillCourseMembers.Where(x => x.CourseID == id).ToList();
            foreach (var user in UserMgr.Users)
            {
                var tempUser = new SkillCourseMember
                {
                    UserID = user.Id,
                    CourseID = (int)id,
                    UserName = user.UserName
                };
                int index = userList.FindIndex(x => x.UserID == user.Id);
                if (index >= 0)
                {
                    tempUser.IsSelected = true;
                }
                else
                {
                    tempUser.IsSelected = false;
                }
                model.Add(tempUser);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUsers(List<SkillCourseMember> courseMembers)
        {
            int courseID = 0;
            foreach (var member in courseMembers.Where(x => x.IsSelected))
            {
                courseID = member.CourseID;
                var tempMember = new SkillCourseMember
                {
                    UserName = member.UserName,
                    CourseID = member.CourseID,
                    UserID = member.UserID,
                    Status = "Enrolled"
                };
                foreach (var oldMem in _context.SkillCourseMembers.Where(x => (x.CourseID == member.CourseID) && (x.UserID == member.UserID) && (x.Status != "Completed")))
                {
                    _context.Remove(oldMem);
                }
                _context.Add(tempMember);
            }
            foreach (var member in courseMembers.Where(x => !x.IsSelected))
            {
                foreach (var gMem in _context.SkillCourseMembers.Where(x => (x.CourseID == member.CourseID) && (x.UserID == member.UserID) && (x.Status != "Completed")))
                {
                    _context.Remove(gMem);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), "SkillCourses");
        }
	
        public async Task<IActionResult> Join(int id)
        {
            AppUser tempUser = await UserMgr.FindByNameAsync(User.Identity.Name);
            SkillCourse tempCourse = await _sccontext.Courses.FindAsync(id);
            int index = 0;

            foreach (var member in _context.SkillCourseMembers.Where(x => (x.CourseID == id) && (x.UserName == User.Identity.Name)))
            {
                index++;
            }
            if (index == 0)
            {
                SkillCourseMember model = new SkillCourseMember
                {
                    UserName = User.Identity.Name,
                    UserID = tempUser.Id,
                    CourseID = id,
                    CourseName = tempCourse.CourseName,
                    DaysCompleted = 0,
                    Status = "Enrolled",
                    CompletionDate = DateTime.MinValue
                };
                try
                {
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    Console.WriteLine("Cannot join the course: An exception occured!");
                }
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_CourseJoinSuccess + tempCourse.CourseName + " !";
                return RedirectToAction(nameof(Index), "SkillCourses");
            }
            TempData["ActionResult"] = Resources.ActionMessages.ActionResult_CourseJoinFail;
            return RedirectToAction(nameof(Index), "SkillCourses");
        }

        public async Task<IActionResult> Complete(int id)
        {
            AppUser tempUser = await UserMgr.FindByNameAsync(User.Identity.Name);
            SkillCourse tempCourse = await _sccontext.Courses.FindAsync(id);

            try
            {
                var member = _context.SkillCourseMembers.Where(x => (x.UserID == tempUser.Id) && (x.CourseID == id)).First();
                member.CompletionDate = DateTime.Now;
                member.Status = "Completed";
                member.DaysCompleted = tempCourse.Length;
                _context.Update(member);
                await _context.SaveChangesAsync();
            }
            catch
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_CourseCompleteFail;
                return RedirectToAction(nameof(Index), "SkillCourses");
            }
            TempData["ActionResult"] = Resources.ActionMessages.ActionResult_CourseCompleteSuccess;
            return RedirectToAction(nameof(Index), "SkillCourses");
        }

        [HttpGet]
        public async Task<IActionResult> Grade(int id)
        {
            var model = new CourseMemberVM
            {
                Id = id
            };
            return View(model);
        }
       
        [HttpPost]
        public async Task<IActionResult> Grade([Bind("Id, CourseGrade")]CourseMemberVM member)
        {
            try
            {
                var tempMember = _context.SkillCourseMembers.Find(member.Id);
                tempMember.CourseGrade = member.CourseGrade;
                _context.Update(tempMember);
                await _context.SaveChangesAsync();
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GradeSuccess;
                return RedirectToAction(nameof(Index), "SkillCourseMembers", new { id = tempMember.CourseID, courseName = tempMember.CourseName });
            }
            catch
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GradeFail;
                return RedirectToAction(nameof(Index), "SkillCourses");
            }
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
        public async Task<IActionResult> Edit(int id, [Bind("Id, UserID, UserName, CourseName, CourseID, Status, CompletionDate, DaysCompleted")] SkillCourseMember skillCourseMember)
        {
            if (id != skillCourseMember.Id)
            {
                return NotFound();
            }

	    if(User.IsInRole("Admin") || User.Identity.Name == skillCourseMember.UserName)
	    {
		if (ModelState.IsValid)
		{
		    try
		    {
			if(skillCourseMember.Status == "Completed")
			{
			    skillCourseMember.CompletionDate = DateTime.Now;
			    var lecourse = await _sccontext.Courses.FirstOrDefaultAsync(m => m.id == skillCourseMember.CourseID);
			    skillCourseMember.DaysCompleted = lecourse.Length;
                    }
			else
			{
			    skillCourseMember.CompletionDate = DateTime.MinValue;
			}
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
		    return RedirectToAction(nameof(Index), "SkillCourseMembers", new { id = skillCourseMember.CourseID });//redirecting back to the list of course members,
		}		
	    }
	    
            return View(skillCourseMember);
        }

        // GET: SkillCourseMembers/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
