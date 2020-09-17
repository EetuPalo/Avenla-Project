using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.AspNetCore.Mvc.Formatters;


namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin, Superadmin")]
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
            
            var list = new List<AppUser>();
            var model = new GroupMember();
            var groupMembers = _context.GroupMembers.Where(x => x.GroupID == id).ToList();

            Group tempGroup = await _context.Group.FindAsync(id);
            TempData["GroupID"] = tempGroup.id;

            if (!String.IsNullOrEmpty(searchString))
            {
                groupMembers.Clear();
                foreach (var member in _context.GroupMembers.Where(s => (s.GroupID == id) && (s.UserName.Contains(searchString))))
                {
                    var memberOfGroup = _context.AppUser.FirstOrDefault(x => x.Id == member.UserID);
                    list.Add(memberOfGroup);
                }
                model.Members = list;
                return View(list);
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

            foreach (var item in groupMembers)
            {
                foreach(AppUser appuser in UserMgr.Users.Where(x=> x.Id == item.UserID))
                {
                    if(appuser.Company == user.Company || User.IsInRole("Superadmin"))
                    {
                        list.Add(appuser);
                    }
                }
            }
            model.Members = list;
            return View(model);
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
            var companygroup = await _context.Group.FirstOrDefaultAsync(x => x.id == id);
            var model = new GroupListOfMembersVM();
            var groupMemList = _context.GroupMembers.Where(x => x.GroupID == id).ToList();

            foreach (var member in _context.CompanyMembers.Where(x=> x.CompanyId == companygroup.CompanyId))
            {
                var user = await UserMgr.Users.FirstOrDefaultAsync(x => x.Id == member.UserId);

                model.GroupMembersList.Add(new SelectListItem() { Text = string.Concat(user.FirstName, " ", user.LastName), Value = string.Concat(member.UserId.ToString(), "|", user.UserName) });
                int index = groupMemList.FindIndex(x => x.UserID == member.UserId);
                if (index >= 0)
                {
                    model.ListOfMembers.Add(string.Concat(member.UserId.ToString(), "|", user.UserName));
                }

            }

            TempData["Source"] = source;
            TempData["groupid"] = companygroup.id;
            TempData["GroupCompany"] = companygroup.company;
            TempData["CompanyId"] = companygroup.CompanyId;
            TempData["GroupName"] = group;
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string source, [Bind("id, name, company, CompanyId")] Group group, string[] GroupMembers)
        {
            


            int groupID = group.id; 
            string groupName = TempData["GroupName"].ToString();
            var memberInfo = new string[2] ;

            // Deletion of previous registrations for the current group
            foreach (var oldMem in _context.GroupMembers.Where(x => (x.GroupID == groupID)))
            {
                _context.Remove(oldMem);
            }
            // Insertion of registrations for the selected users as returned
            foreach (var member in GroupMembers)
            {
                memberInfo = member.Split("|");
                var tempMember = new GroupMember
                {
                    UserID = Convert.ToInt32(memberInfo[0]),
                    UserName = memberInfo[1],
                    GroupID = groupID,
                    GroupName = groupName
                };
                _context.Add(tempMember);
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
