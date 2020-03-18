
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;

namespace Login_System.Controllers
{
    public class SkillCoursesController : Controller
    {
        private readonly SkillCourseDataContext _context;
        private readonly SkillCourseMemberDataContext memberContext;

        public SkillCoursesController(SkillCourseDataContext context, SkillCourseMemberDataContext memCon)
        {
            _context = context;
            memberContext = memCon;
        }

        // GET: SkillCourses
        public async Task<IActionResult> Index(string searchString)
        {
            var courses = from c in _context.Courses select c;
            if(!String.IsNullOrEmpty(searchString))
            {
                courses = courses.Where(s => (s.CourseName.Contains(searchString) || s.CourseContents.Contains(searchString)));
            }
            return View(await courses.ToListAsync().ConfigureAwait(false));
        }

        // GET: SkillCourses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillCourse = await _context.Courses
                .FirstOrDefaultAsync(m => m.id == id).ConfigureAwait(false);
            if (skillCourse == null)
            {
                return NotFound();
            }

            return View(skillCourse);
        }

        // GET: SkillCourses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SkillCourses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,CourseName,CourseContents, Location, Length")] SkillCourse skillCourse)
        {
            if (ModelState.IsValid)
            {
                _context.Add(skillCourse);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return RedirectToAction(nameof(Index));
            }
            return View(skillCourse);
        }

        // GET: SkillCourses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillCourse = await _context.Courses.FindAsync(id);
            if (skillCourse == null)
            {
                return NotFound();
            }
            return View(skillCourse);
        }

        // POST: SkillCourses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,CourseName,CourseContents, Location, Length")] SkillCourse skillCourse)
        {
            if (skillCourse != null && id != skillCourse.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var member in memberContext.SkillCourseMembers.Where(x => x.CourseID == skillCourse.id))
                    {
                        member.CourseName = skillCourse.CourseName;
                        memberContext.Update(member);
                    }
                    await memberContext.SaveChangesAsync().ConfigureAwait(false);
                }
                catch
                {

                }

                try
                {
                    _context.Update(skillCourse);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillCourseExists(skillCourse.id))
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
            return View(skillCourse);
        }

        // GET: SkillCourses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillCourse = await _context.Courses
                .FirstOrDefaultAsync(m => m.id == id);
            if (skillCourse == null)
            {
                return NotFound();
            }

            return View(skillCourse);
        }

        // POST: SkillCourses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skillCourse = await _context.Courses.FindAsync(id);
            _context.Courses.Remove(skillCourse);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SkillCourseExists(int id)
        {
            return _context.Courses.Any(e => e.id == id);
        }
    }
}
