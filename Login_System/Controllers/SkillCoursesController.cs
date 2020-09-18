using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography.X509Certificates;

namespace Login_System.Controllers
{
    public class SkillCoursesController : Controller
    {
        private readonly GeneralDataContext _context;
        private readonly UserManager<AppUser> UserMgr;

        public SkillCoursesController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }

        // GET: SkillCourses
        public async Task<IActionResult> Index(string searchString)
        {
            var model = new SkillCoursesVM();
            var tempDuration = new Dictionary<int, string>();
            var courses = from c in _context.Courses select c;
            TempData["SearchString"] = Resources.Courses.Index_Search;
            TempData["SearchValue"] = null;

            if (!String.IsNullOrEmpty(searchString))
            {
                courses = courses.Where(s => (s.CourseName.Contains(searchString) || s.CourseContents.Contains(searchString)));
                TempData["SearchValue"] = searchString;
            }

            //Checking if user has already enrolled on a course
            AppUser tempUser = await UserMgr.GetUserAsync(User);

            foreach (var course in courses)
            {
                var dateList = new List<DateTime>();

                // Clearing the datelist for every course
                dateList.Clear();

                foreach (var member in _context.SkillCourseMembers.Where(x => (x.UserID == tempUser.Id) && (x.CourseID == course.id)))
                {
                    course.MemberStatus = true;
                    if (member.Status == "Completed")
                    {
                        course.CompleteStatus = true;
                    }
                }

                foreach (var tempLesson in _context.Lessons.Where(x => x.CourseID == course.id))
                {
                    dateList.Add(tempLesson.Date);
                }

                if (dateList.Count > 0)
                {
                    string minDate = dateList.Min().ToShortDateString();
                    string maxDate = dateList.Max().ToShortDateString();
                    string duration = minDate + " - " + maxDate;
                    tempDuration.Add(course.id, duration);
                }

                else
                {
                    tempDuration.Add(course.id, "No duration set");
                }              
            }

            model.Courses = courses.ToList();            
            var lessons = _context.Lessons.ToList();

            foreach (var lesson in lessons)
            {
                foreach (var user in _context.LessonUsers.Where(x => (x.MemberID == tempUser.Id)))
                {
                    if (user.LessonID == lesson.Id)
                    {
                        lesson.LessonStatus = true;
                    }

                    else
                    {
                        if (lesson.LessonStatus != true)
                        {
                            lesson.LessonStatus = false;
                        }
                    }
                }
            }
            model.Lessons = lessons;
            model.Durations = tempDuration;

            return View(model);
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
	    [Authorize(Roles = "Admin, Superadmin")]
        public IActionResult Create()
        {
            var model = new SkillCoursesVM();
            foreach (var skill in _context.Skills)
            {
                model.SkillList.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Id.ToString() });
            }

            return View(model);
        }

        // POST: SkillCourses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Create([Bind("id,CourseName,CourseContents, Location, Length, Skill, goal, startLevel")] SkillCoursesVM skillCourse)
        {
            if (ModelState.IsValid)
            {
                var course = new SkillCourse
                {
                    CourseContents = skillCourse.CourseContents,
                    CourseName = skillCourse.CourseName,
                    Location = skillCourse.Location,
                    Length = skillCourse.Length
                };
                _context.Add(course);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                if(skillCourse.Skill[0] != null)
                {
                    int index = 0;
                    foreach (var skillId in skillCourse.Skill)
                    {
                        var skill = await _context.Skills.FirstOrDefaultAsync(x => x.Id == int.Parse(skillId));

                        _context.Add(new SkillsInCourse
                        {
                            SkillId = skill.Id,
                            CourseId = course.id,
                            SkillGoal = int.Parse(skillCourse.goal[index]),
                            SkillStartingLevel = int.Parse(skillCourse.startLevel[index]),

                        });
                        index++;

                    }
                    await _context.SaveChangesAsync();
                }
               
                return RedirectToAction(nameof(Index));
            }
            return View(skillCourse);
        }

        // GET: SkillCourses/Edit/5
        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            SkillCoursesVM model = new SkillCoursesVM();
            if (id == null)
            {
                return NotFound();
            }

            var skillCourse = await _context.Courses.FindAsync(id);

           // model.id = skillCourse.id;
            model.skillCourse = skillCourse;
            
            if (skillCourse == null)
            {
                return NotFound();
            }
            foreach (var skill in _context.Skills)
            {
                model.SkillList.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Id.ToString() });
            }
            var goalList = new List<int>();
            var startList = new List<int>();
            var skillList = new List<int>();
            foreach(var skill in _context.SkillsInCourse.Where(x=> x.CourseId == id))
            {
                goalList.Add(skill.SkillGoal);
                startList.Add(skill.SkillStartingLevel);
                skillList.Add(skill.SkillId);
            }
            ViewBag.goalList = goalList.ToArray();
            ViewBag.startList = startList.ToArray();
            ViewBag.skillList = skillList.ToArray();

            return View(model);
        }

        // POST: SkillCourses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Edit(int id, [Bind("id,CourseName,CourseContents, Location, Length, Skill, goal, startLevel")] SkillCoursesVM skillCourse)
        {
            int index = 0;
            if (skillCourse != null && id != skillCourse.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
               
                    foreach (var member in _context.SkillCourseMembers.Where(x => x.CourseID == skillCourse.id))
                    {
                        member.CourseName = skillCourse.CourseName;
                        _context.Update(member);
                    }
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }

                catch
                {

                }

                try
                {
                    var course = _context.Courses.FirstOrDefault(x=> x.id == skillCourse.id);
                    course.CourseName = skillCourse.CourseName;
                    course.Location = skillCourse.Location;
                    course.Length = skillCourse.Length;
                    course.CourseContents = skillCourse.CourseContents;
                    _context.Update(course);
                    List<SkillsInCourse> oldSkillList = _context.SkillsInCourse.Where(x=> x.CourseId == skillCourse.id).ToList();
                    foreach(var skill in oldSkillList)
                    {
                        if (!skillCourse.Skill.Contains(skill.SkillId.ToString()))
                        {
                            Console.Write("Here I Am!");
                            _context.Remove(skill);
                            
                        }
                    }
                    foreach(var item in skillCourse.Skill)
                    {
                        var skill = _context.Skills.FirstOrDefault(x => x.Id == int.Parse(item));
                        var courseSkill = _context.SkillsInCourse.FirstOrDefault(x => x.SkillId == skill.Id && x.CourseId == skillCourse.id);
                        if(courseSkill != null)
                        {
                            courseSkill.SkillGoal = int.Parse(skillCourse.goal[index]);
                            courseSkill.SkillStartingLevel = int.Parse(skillCourse.startLevel[index]);
                            _context.Update(courseSkill);
                        }
                        else
                        {
                            _context.Add(new SkillsInCourse
                            {
                                CourseId = skillCourse.id,
                                SkillGoal = int.Parse(skillCourse.goal[index]),
                                SkillStartingLevel = int.Parse(skillCourse.startLevel[index]),
                                SkillId = int.Parse(item)

                            });
                        }

                        index++;
                    }
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
        [Authorize(Roles = "Admin, Superadmin")]
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
        [Authorize(Roles = "Admin, Superadmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skillCourse = await _context.Courses.FindAsync(id);
            _context.Courses.Remove(skillCourse);
            foreach(var item in _context.SkillsInCourse.Where(x => x.CourseId == id))
            {
                _context.SkillsInCourse.Remove(item);
            }
            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool SkillCourseExists(int id)
        {
            return _context.Courses.Any(e => e.id == id);
        }
    }
}
