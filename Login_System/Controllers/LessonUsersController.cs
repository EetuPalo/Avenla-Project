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
using System.Globalization;

namespace Login_System.Controllers
{
    public class LessonUsersController : Controller
    {
        private readonly LessonUserDataContext _context;
        private readonly UserManager<AppUser> UserMgr;
        private readonly SkillCourseMemberDataContext courseMemberContext;

        public LessonUsersController(LessonUserDataContext context, UserManager<AppUser> userManager, SkillCourseMemberDataContext courseMemCon)
        {
            _context = context;
            UserMgr = userManager;
            courseMemberContext = courseMemCon;
        }

        // GET: LessonUsers
        public async Task<IActionResult> Index(int id)
        {
            return View(await _context.LessonUsers.Where(x => x.LessonID == id).ToListAsync());
        }

        // GET: LessonUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lessonUser = await _context.LessonUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lessonUser == null)
            {
                return NotFound();
            }

            return View(lessonUser);
        }

        // GET: LessonUsers/Create
	[Authorize(Roles = "Admin")]
        public IActionResult Create(int id, int courseId)
        {
            var model = new List<LessonUser>();
            var userList = _context.LessonUsers.Where(x => x.LessonID == id).ToList();
            foreach (var user in courseMemberContext.SkillCourseMembers.Where(x => x.CourseID == courseId))
            {
                var tempUser = new LessonUser
                {
                    MemberID = user.Id,
                    LessonID = (int)id,
                    MemberName = user.UserName
                };
                int index = userList.FindIndex(x => x.MemberID == user.Id);
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

        // POST: LessonUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<LessonUser> lessonUsers)
        {
            int lessonID = 0;
            foreach (var member in lessonUsers.Where(x => x.IsSelected))
            {
                lessonID = member.LessonID;
                var tempMember = new LessonUser
                {
                    MemberName = member.MemberName,
                    LessonID = member.LessonID,
                    MemberID = member.MemberID,
                    Attending = true
                };
                foreach (var oldMem in _context.LessonUsers.Where(x => (x.LessonID == member.LessonID) && (x.MemberID == member.MemberID)))
                {
                    _context.Remove(oldMem);
                }
                _context.Add(tempMember);
            }
            foreach (var member in lessonUsers.Where(x => !x.IsSelected))
            {
                foreach (var gMem in _context.LessonUsers.Where(x => (x.LessonID == member.LessonID) && (x.MemberID == member.MemberID)))
                {
                    _context.Remove(gMem);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), "SkillCourses");
        }

        // GET: LessonUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lessonUser = await _context.LessonUsers.FindAsync(id);
            if (lessonUser == null)
            {
                return NotFound();
            }
            return View(lessonUser);
        }

        // POST: LessonUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LessonID,MemberID,MemberName,Attending")] LessonUser lessonUser)
        {
            if (id != lessonUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lessonUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonUserExists(lessonUser.Id))
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
            return View(lessonUser);
        }

        // GET: LessonUsers/Delete/5
	[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lessonUser = await _context.LessonUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lessonUser == null)
            {
                return NotFound();
            }

            return View(lessonUser);
        }

        // POST: LessonUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
	[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lessonUser = await _context.LessonUsers.FindAsync(id);
            _context.LessonUsers.Remove(lessonUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LessonUserExists(int id)
        {
            return _context.LessonUsers.Any(e => e.Id == id);
        }
    }
}
