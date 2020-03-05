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
    public class GroupsController : Controller
    {
        private readonly GroupsDataContext _context;
        private readonly GroupMembersDataContext gMemContext;
        private readonly SkillGoalContext goalContext;

        public GroupsController(GroupsDataContext context, GroupMembersDataContext gMemberCon, SkillGoalContext skillGoalCon)
        {
            _context = context;
            gMemContext = gMemberCon;
            goalContext = skillGoalCon;
        }

        // GET: Groups
        public async Task<IActionResult> Index(string searchString)
        {
            var groups = from g in _context.Group select g;
            if (!String.IsNullOrEmpty(searchString))
            {
                groups = groups.Where(s => s.name.Contains(searchString));
            }
            return View(await groups.ToListAsync());
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Group
                .FirstOrDefaultAsync(m => m.id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // GET: Groups/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,name")] Group @group)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@group);
                await _context.SaveChangesAsync();
                TempData["ActionResult"] = "Group created! Next you need to set up the group goals.";
                TempData["ActionPhase"] = "[2/3]";
                TempData["Source"] = "create";
                return RedirectToAction(nameof(Create), "SkillGoals", new { name = group.name });
            }
            TempData["ActionResult"] = "Error!";           
            return View(@group);
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Group.FindAsync(id);
            if (@group == null)
            {
                return NotFound();
            }
            return View(@group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,name")] Group @group)
        {
            if (id != @group.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.id))
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
            return View(@group);
        }

        // GET: Groups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Group
                .FirstOrDefaultAsync(m => m.id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @group = await _context.Group.FindAsync(id);

            //Removes all groupMember and skillgoals associations with the group, so we are not left with phantom data in the database

            foreach (var groupMember in gMemContext.GroupMembers)
            {
                if (groupMember.GroupID == group.id)
                {
                    gMemContext.Remove(groupMember);
                }
            }
            await gMemContext.SaveChangesAsync();
            foreach (var goal in goalContext.SkillGoals)
            {
                if (goal.GroupName == group.name)
                {
                    goalContext.Remove(goal);
                }
            }
            await goalContext.SaveChangesAsync();

            _context.Group.Remove(@group);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(int id)
        {
            return _context.Group.Any(e => e.id == id);
        }
    }
}
