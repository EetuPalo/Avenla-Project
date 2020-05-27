using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GroupMembersController : Controller
    {
        private readonly GeneralDataContext _context;
        private readonly UserManager<AppUser> UserMgr;

        public GroupMembersController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }

        // GET: GroupMembers
        public async Task<IActionResult> Index(int? id, string searchString)
        {
            var user = await UserMgr.GetUserAsync(HttpContext.User);
            ViewBag.CurrentCompany = user.Company;

            var list = new List<GroupMember>();
            var model = _context.GroupMembers.Where(x => x.GroupID == id).ToList();

            Group tempGroup = await _context.Group.FindAsync(id);
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

            foreach (var item in model)
            {
                foreach(AppUser appuser in UserMgr.Users.Where(x=> x.Id == item.UserID))
                {
                    if(appuser.Company == user.Company)
                    {
                        list.Add(item);
                    }
                }
            }
            return View(list);
        }

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

#nullable enable
        public async Task<IActionResult> Create(string? group, string? source, int id)
        {
            var currentUser = await UserMgr.GetUserAsync(HttpContext.User);
            var model = new List<GroupUser>();
            var groupMemList = _context.GroupMembers.Where(x => x.GroupID == id).ToList();
            foreach (var user in UserMgr.Users.Where(x=> x.Company == currentUser.Company))
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
                //This ensures we don't add duplicate members
                foreach (var oldMem in _context.GroupMembers.Where(x => (x.GroupID == member.GroupId) && (x.UserID == Convert.ToInt32(member.UserId))))
                {
                    _context.Remove(oldMem);
                }
                _context.Add(tempMember);               
           }
           //Those who aren't selected will be ignored, unless they are currently members of the group. In that case they will be removed
           foreach (var member in groupMembers.Where(x => !x.IsSelected))
           {
                foreach (var gMem in _context.GroupMembers.Where(x => (x.GroupID == member.GroupId) && (x.UserID.ToString() == member.UserId)))
                {
                    _context.Remove(gMem);
                }
           }
           _context.SaveChanges();

            //Two possibilities for what the next page is
            //One for continuing the group creation process, the other for going to the groupmembers list
            if (source == "create")
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GroupCreatedNoGoal;
                return RedirectToAction(nameof(Index), "Groups");
            }
            else
            {
                return RedirectToAction(nameof(Index), "GroupMembers", new { id = groupID });
            }
        }
        
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
