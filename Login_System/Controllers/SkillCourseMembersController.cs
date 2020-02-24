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
    public class SkillCourseMembersController : Controller
    {
        private readonly SkillCourseMemberDataContext _context;

        public SkillCourseMembersController(SkillCourseMemberDataContext context)
        {
            _context = context;
        }

        // GET: SkillCourseMembers
        public async Task<IActionResult> Index()
        {
            return View(await _context.SkillCourseMembers.ToListAsync());
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: SkillCourseMembers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserID,UserName")] SkillCourseMember skillCourseMember)
        {
            if (ModelState.IsValid)
            {
                _context.Add(skillCourseMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserID,UserName")] SkillCourseMember skillCourseMember)
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
            return RedirectToAction(nameof(Index));
        }

        private bool SkillCourseMemberExists(int id)
        {
            return _context.SkillCourseMembers.Any(e => e.Id == id);
        }
    }
}
