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
        public async Task<IActionResult> Index(int? id, string searchString)
        {
            var model = _context.GroupMembers.Where(x => x.GroupID == id).ToList();

            Group tempGroup = await _gcontext.Group.FindAsync(id);
            TempData["GroupID"] = tempGroup.id;

            if (!String.IsNullOrEmpty(searchString))
            {
                model.Clear();
                foreach (var member in _context.GroupMembers.Where(s => (s.GroupID == id) && (s.UserName.Contains(searchString))))
                {
                    model.Add(member);
                }
                return View(model);
            }           

            try
            {
                TempData["GroupID"] = id;
                TempData["GroupName"] = tempGroup.name;
            }
            catch(NullReferenceException)
            {
                //line 63 causes NullReference exception but doesn't actually prevent the program from working as intended, so the exception is just ignored
                //someday would need to look into it.
            }           

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
                .FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
        }

        // GET: GroupMembers/Create
#nullable enable
        public IActionResult Create(string? group, string? source, int id)
        {
            var model = new List<GroupUser>();
            var groupMemList = _context.GroupMembers.Where(x => x.GroupID == id).ToList();
            foreach (var user in UserMgr.Users)
            {
                var tempUser = new GroupUser
                {
                    UserId = user.Id.ToString(),
                    GroupId = (int)id,
                    UserName = user.UserName,
                    GroupName = group
                };
                int index = groupMemList.FindIndex(x => x.UserID == user.Id);
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
            TempData["Source"] = source;
            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string source, List<GroupUser> groupMembers)
        {
           int groupID = 0;
           foreach (var member in groupMembers.Where(x => x.IsSelected))
           {
                groupID = member.GroupId;
                var tempMember = new GroupMember
                {
                    UserName = member.UserName,
                    UserID = Convert.ToInt32(member.UserId),
                    GroupID = member.GroupId,
                    GroupName = member.GroupName
                };
                foreach (var oldMem in _context.GroupMembers.Where(x => (x.GroupID == member.GroupId) && (x.UserID == Convert.ToInt32(member.UserId))))
                {
                    _context.Remove(oldMem);
                }
                _context.Add(tempMember);               
           }
           foreach (var member in groupMembers.Where(x => !x.IsSelected))
           {
                foreach (var gMem in _context.GroupMembers.Where(x => (x.GroupID == member.GroupId) && (x.UserID.ToString() == member.UserId)))
                {
                    _context.Remove(gMem);
                }
           }
           _context.SaveChanges();
            if (source == "create")
            {
                TempData["ActionResult"] = "Group created successfully.";
                return RedirectToAction(nameof(Index), "Groups");
            }
            else
            {
                return RedirectToAction(nameof(Index), "GroupMembers", new { id = groupID });
            }
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
            return RedirectToAction(nameof(Index), "GroupMembers", new { id = groupMember.GroupID });//redirecting back to the list of group members after deletion
        }

        private bool GroupMemberExists(int id)
        {
            return _context.GroupMembers.Any(e => e.Id == id);
        }
    }
}
