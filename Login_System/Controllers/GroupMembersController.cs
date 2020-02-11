using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Login_System.Controllers
{
    public class GroupMembersController : Controller
    {
        private readonly GroupMembersDataContext _context;
        private readonly GroupsDataContext _gcontext;
        private readonly UserManager<AppUser> UserMgr;

        public GroupMembersController(GroupMembersDataContext context, UserManager<AppUser> userManager, GroupsDataContext groups)
        {
            _context = context;
            UserMgr = userManager;
            _gcontext = groups;
        }

        // GET: GroupMembers
        public async Task<IActionResult> Index(int? id)
        {
            var model = new List<GroupMemberVM>();
            //if (id == null)
            //{
            //    Console.WriteLine("DEBUG: No ID has been passed to the controller. Listing the skills of the currently logged in user.");
            //    id = Convert.ToInt32(UserMgr.GetUserId(User));
            //}

            //fetch current user from the users table
            //AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            //fetch current user from the grouptable table to get groupid of his group
            //GroupMember whatever = await _context.GroupMembers.FindAsync(id);
            //tempUser = await UserMgr.FindByIdAsync(whatever.UserID.ToString());
            Group tempGroup = await _gcontext.Group.FindAsync(id);
            //for loop to iterate through members, but only show current user for now, later will show all group user partakes in(if several)
            foreach(var member in _context.GroupMembers)
            {
                if (member.GroupID == id)
                {
                    var grpmember = new GroupMemberVM();
                    grpmember.Id = member.Id;
                    grpmember.UserID = member.UserID;
                    AppUser tempUser = await UserMgr.FindByIdAsync(grpmember.UserID.ToString());
                    grpmember.UserName = tempUser.UserName;
                    grpmember.GroupName = tempGroup.name;

                    model.Add(grpmember);
                }
            }

            //Some information that we might want to use elsewhere
            //TempData["UserId"] = id;
            //TempData["UserName"] = tempUser.UserName;

            return View(model);
        }

        // GET: GroupMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
        }

        // GET: GroupMembers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GroupMembers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserID,GroupID")] GroupMember groupMember)
        {
            if (ModelState.IsValid)
            {
                _context.Add(groupMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(groupMember);
        }

        // GET: GroupMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMembers.FindAsync(id);
            if (groupMember == null)
            {
                return NotFound();
            }
            return View(groupMember);
        }

        // POST: GroupMembers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserID,GroupID")] GroupMember groupMember)
        {
            if (id != groupMember.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupMemberExists(groupMember.Id))
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
            return View(groupMember);
        }

        // GET: GroupMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
        }

        // POST: GroupMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var groupMember = await _context.GroupMembers.FindAsync(id);
            _context.GroupMembers.Remove(groupMember);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupMemberExists(int id)
        {
            return _context.GroupMembers.Any(e => e.Id == id);
        }
    }
}
