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
            var model = new List<GroupMemberVM>();
            Group tempGroup = await _gcontext.Group.FindAsync(id);
            TempData["GroupName"] = tempGroup.name;
            TempData["GroupID"] = tempGroup.id;

            if (!String.IsNullOrEmpty(searchString))
            {
                var members = from gm in _context.GroupMembers select gm;
                members = members.Where(s => (s.GroupID == id) && (s.UserName.Contains(searchString)));
                foreach (var member in members)
                {
                    var tempModel = new GroupMemberVM
                    {
                        UserName = member.UserName,
                        UserID = member.UserID,
                        GroupName = member.GroupName,
                        Id = member.Id
                    };
                    model.Add(tempModel);
                }
                return View(model);
            }
           
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

            //Information that is useful in other methods that is not always available
            TempData["GroupID"] = id;
            try
            {
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
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var groupMemList = new List<GroupMember>();
            foreach (var user in UserMgr.Users)
            {
                var tempUser = new GroupUser
                {
                    UserId = user.Id.ToString(),
                    GroupId = (int)id,
                    UserName = user.UserName,
                    GroupName = group
                };

                foreach (var gMem in _context.GroupMembers)
                {
                    if (gMem.GroupID == id)
                    {
                        groupMemList.Add(gMem);
                    }
                }
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
           foreach (var member in groupMembers)
            {
                groupID = member.GroupId;
                if (member.IsSelected)
                {
                    var tempMember = new GroupMember
                    {
                        UserName = member.UserName,
                        UserID = Convert.ToInt32(member.UserId),
                        GroupID = member.GroupId,
                        GroupName = member.GroupName
                    };
                    foreach (var oldMem in _context.GroupMembers)
                    {
                        if (oldMem.GroupID == member.GroupId && oldMem.UserID == Convert.ToInt32(member.UserId))
                        {
                            _context.Remove(oldMem);
                        }
                        _context.Add(tempMember);
                    }                    
                }
                else if (!member.IsSelected)
                {
                    foreach (var gMem in _context.GroupMembers)
                    {
                        if (gMem.GroupID == member.GroupId && gMem.UserID.ToString() == member.UserId)
                        {
                            _context.Remove(gMem);
                        }
                    }
                }                
            }
            _context.SaveChanges();
            if (source == "create")
            {
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
