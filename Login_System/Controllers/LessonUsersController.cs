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
    public class LessonUsersController : Controller
    {
        private readonly LessonUserDataContext _context;

        public LessonUsersController(LessonUserDataContext context)
        {
            _context = context;
        }

        // GET: LessonUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.LessonUsers.ToListAsync());
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: LessonUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LessonID,MemberID,MemberName,Attending")] LessonUser lessonUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lessonUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(lessonUser);
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
