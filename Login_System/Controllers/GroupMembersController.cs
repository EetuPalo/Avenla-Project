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
        public IActionResult Create(string? group, string? source, int? id)
        {
            var member = UserMgr.Users.ToList();            
            if(id != null)
            {
                var model = new GroupMember();
                {
                    model.Uname = member.Select(x => new SelectListItem
                    {
                        Value = x.UserName,
                        Text = x.UserName
                    });//creating a list of dropdownlist elements                 
                    model.GroupID = (int)id;//assigning GroupID of the current group
                    model.GroupName = group;
                };
                TempData["Source"] = source;
                return View(model);
            }
            else
            {
                id = TempData["GroupID"] as int?;//using groupid we saved earlier in the Index if the id passed to the method is NULL
                var model = new GroupMember();
                {                    
                    model.Uname = member.Select(x => new SelectListItem
                    {
                        Value = x.UserName,
                        Text = x.UserName
                    });                   
                    model.GroupID = (int)id;//assigning GroupID of the current group
                    model.GroupName = TempData["GroupName"] as string;//assigning groupname that we saved as well
                    TempData.Keep();//so the data is not lost because it's TEMPdata (temporary)
                };
                TempData["Source"] = source;
                return View(model);
            }            
        }

        // POST: GroupMembers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string source, [Bind("UserID,GroupID, UserName, GroupName")] GroupMember groupMember)
        {
            if (ModelState.IsValid)
            {
                var user = await UserMgr.FindByNameAsync(groupMember.UserName);//creating a temp user through username selected in the view
                groupMember.UserID = user.Id;//assinging UserID of the selected user
                //groupMember.GroupID = Convert.ToInt32(TempData["GroupID"]);//the id in the temp data is not int so we convert it
                //groupMember.GroupName = TempData["GroupName"] as string;//same as id
                //TempData.Keep();//keeping the temp data otherwise, groupMember won't have groupid and groupname
                _context.Add(groupMember);
                await _context.SaveChangesAsync();
                if (source == "create")
                {
                    TempData["ActionResult"] = "Group created successfully!";
                    return RedirectToAction(nameof(Index), "Groups");
                }
                return RedirectToAction(nameof(Index), "GroupMembers", new { id = groupMember.GroupID});//redirecting back to the list of group members,
                // without specifying the id, an empty list is shown
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
            return RedirectToAction(nameof(Index), "GroupMembers", new { id = groupMember.GroupID });//redirecting back to the list of group members after deletion
        }

        private bool GroupMemberExists(int id)
        {
            return _context.GroupMembers.Any(e => e.Id == id);
        }
    }
}
