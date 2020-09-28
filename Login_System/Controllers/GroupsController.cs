 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography.X509Certificates;

namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin, Superadmin")]
    public class GroupsController : Controller
    {
        private readonly GeneralDataContext _context;
        private readonly UserManager<AppUser> UserMgr;

        public GroupsController(GeneralDataContext context, UserManager<AppUser> usermgr)
        {
            _context = context;
            UserMgr = usermgr;
        }

        // GET: Groups
        public async Task<IActionResult> Index(string searchString)
        {
            var currentUser = await UserMgr.GetUserAsync(HttpContext.User);
            IQueryable<Group> groups = null;
            if (!User.IsInRole("Superadmin"))
            {
                groups = from g in _context.Group where g.CompanyId == currentUser.Company select g;
            }
            else
            {
                groups = from g in _context.Group select g;

            }
           
            TempData["SearchValue"] = null;
            //SEARCH
            if (!String.IsNullOrEmpty(searchString))
            {
                groups = groups.Where(s => s.name.Contains(searchString));
                TempData["SearchValue"] = searchString;
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
        public async Task<IActionResult> Create(string? group, string? source)
        {
            var user = await UserMgr.GetUserAsync(HttpContext.User);
            if (User.IsInRole("Admin"))
            {
                TempData["CompanyID"] = user.Company;
            }
            var model = new GroupVM();
            foreach (var company in _context.Company)
            {
                model.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Id.ToString()});
            }

            // SKILLS GET
            
            DateTime date = DateTime.Now;

            if (User.IsInRole("Admin"))
            {
                var companygrouplist = new List<CompanyGroups>();
                foreach (var cmpgrpmbrid in _context.CompanyGroupMembers.Where(x => x.CompanyId == user.Company).Select(x => x.CompanyGroupId))
                {
                    var companyGroup = _context.CompanyGroups.FirstOrDefault(x => x.CompanyGroupId == cmpgrpmbrid);
                    if (!companygrouplist.Contains(companyGroup))
                    {
                        companygrouplist.Add(companyGroup);
                    }
                }

                foreach (var companygrp in companygrouplist)
                {
                    foreach (var skillofgroup in _context.CompanyGroupSkills.Where(x => ((x.CompanyGroupId == companygrp.CompanyGroupId) && (x.CompanyId == user.Company)) || ((x.CompanyGroupId == companygrp.CompanyGroupId && (x.CompanyId == (int?)null)))))
                    {
                        var skill = _context.Skills.FirstOrDefault(x => x.Id == skillofgroup.SkillId);
                        model.Skills.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Id.ToString() });
                    }
                }
            }

            if (User.IsInRole("Superadmin"))
            {
                foreach (var skill in _context.Skills)
                {
                    model.Skills.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Id.ToString() });
                }
            }


            // GroupMembers GET

            foreach (var member in _context.CompanyMembers.Where(x => x.CompanyId == user.Company))
            {
                var userID = await UserMgr.Users.FirstOrDefaultAsync(x => x.Id == member.UserId);

                model.GroupMembersList.Add(new SelectListItem() { Text = string.Concat(userID.FirstName, " ", userID.LastName), Value = string.Concat(member.UserId.ToString(), "|", userID.UserName) });

            }
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,name,CompanyId")] Group @group, CreateSkillGoalsVM goals, string[] Skill, string GroupName, int Groupid, string skillName, string[] GroupMembers)
        {
            if (ModelState.IsValid)
            {
                var company = await _context.Company.FirstOrDefaultAsync(x=> x.Id == group.CompanyId);
                var addGroup = new Group
                {
                    company = company.Name,
                    CompanyId = company.Id,
                    name = group.name,

                };

                _context.Add(addGroup);
                await _context.SaveChangesAsync();
                //SKILLS POST

                var x = goals.Groupid;
                //var groupName = _context.Group.FirstOrDefaultAsync(x => x.id == );
                foreach (var skill in Skill)
                {
                    var skillFromTable = await _context.Skills.FirstOrDefaultAsync(x => x.Id == int.Parse(skill));
                    var skillGoal = new SkillGoals
                    {
                        //SkillName = skill.Skill,
                        SkillGoal = -1,
                        Date = DateTime.Now,
                        SkillName = skillFromTable.Skill,
                        SkillId = skillFromTable.Id,
                        GroupName = addGroup.name,
                        GroupId = addGroup.id
                    };
                    _context.Add(skillGoal);
                }
                await _context.SaveChangesAsync();


                // GroupMembers POST

                var memberInfo = new string[2];

                // Insertion of registrations for the selected users as returned
                foreach (var member in GroupMembers)
                {
                    memberInfo = member.Split("|");
                    var tempMember = new GroupMember
                    {
                        UserID = Convert.ToInt32(memberInfo[0]),
                        UserName = memberInfo[1],
                        GroupID = addGroup.id,
                        GroupName = addGroup.name
                    };
                    _context.Add(tempMember);
                }
                await _context.SaveChangesAsync();

            }          
            return RedirectToAction(nameof(Index));
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

#nullable enable
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
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

            //Removes all groupMember and skillgoals associations with the group, so we are not left with phantom data in the database

            foreach (var groupMember in _context.GroupMembers.Where(x => x.GroupID == group.id))
            {
                _context.Remove(groupMember);
            }
            await _context.SaveChangesAsync();
            foreach (var goal in _context.SkillGoals.Where(x => x.GroupName == group.name))
            {
                _context.Remove(goal);
            }
            await _context.SaveChangesAsync();
            _context.Group.Remove(@group);

            await _context.SaveChangesAsync();        

            return Json(new { success = true, message = "Delete successful" });
        }
        //public async Task<IActionResult> Delete(int? id, string? source)
        //{
        //    if (source != null)
        //    {
        //        TempData["Source"] = source;
        //    }

        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var @group = await _context.Group
        //        .FirstOrDefaultAsync(m => m.id == id);
        //    if (@group == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(@group);
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var @group = await _context.Group.FindAsync(id);

        //    //Removes all groupMember and skillgoals associations with the group, so we are not left with phantom data in the database

        //    foreach (var groupMember in _context.GroupMembers.Where(x => x.GroupID == group.id))
        //    {
        //        _context.Remove(groupMember);
        //    }
        //    await _context.SaveChangesAsync();
        //    foreach (var goal in _context.SkillGoals.Where(x => x.GroupName == group.name))
        //    {
        //        _context.Remove(goal);
        //    }
        //    await _context.SaveChangesAsync();
        //    _context.Group.Remove(@group);

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool GroupExists(int id)
        {
            return _context.Group.Any(e => e.id == id);
        }

        public async Task<IActionResult> Statistics(int id)
        {
            var model = new List<GroupStatisticsVM>();

            Group tempGroup = await _context.Group.FindAsync(id);
            var memberList = _context.GroupMembers.Where(g => g.GroupID == id).ToList();
            var goalList = _context.SkillGoals.Where(g => g.GroupId == tempGroup.id).ToList();
            //var test = _context.SkillGoals.Where(x => (x.Groupid == tempGroup.id)).OrderByDescending(x => x.Date).ToList();
            List<int> skillIdList = new List<int>();

            var members = _context.GroupMembers.Where(x => x.GroupID == tempGroup.id).Select(x=> x.UserID).ToList();
            var userSkills = _context.UserSkills.ToList();           
            var userSkillList = new Dictionary<int, List<DateTime>>();
            var maxDateList = new Dictionary<int, DateTime>();
            var skillAvgList = new Dictionary<string, List<int>>();           

            //Messages that are shown at the top of the page
            ViewBag.GroupName = tempGroup.name;
            TempData["GroupID"] = tempGroup.id;
            //General data about the group
            ViewBag.MemberCount = memberList.Count(m => m.GroupID == id);

            // var skillsInGroup = bb
            //---------------new-------------//
            var skillsInGroup = _context.SkillGoals.Where(x=>x.GroupId == tempGroup.id);
            List<Skills> groupSkillGoals = new List<Skills>();
            foreach (var item in skillsInGroup)
            {
                Skills skill = _context.Skills.FirstOrDefault(x => x.Id == item.SkillId);
                if (!groupSkillGoals.Contains(skill))
                {
                    groupSkillGoals.Add(skill);
                }
                
            }

            foreach(var skill in groupSkillGoals)
            {
                List<UserSkills> userskills = new List<UserSkills>(); 
                foreach (var item in _context.UserSkills.Where(x => (x.SkillId == skill.Id)&& (members.Contains(x.UserID))).OrderByDescending(x => x.Date))
                {
                    if(!userskills.Any(x=> x.SkillId == skill.Id && x.UserID == item.UserID))
                    {
                        userskills.Add(item);
                    }
                }
                var skillGoal = _context.SkillGoals.OrderByDescending(x => x.Date).FirstOrDefault(x => x.SkillId == skill.Id && x.GroupId == tempGroup.id);
                var tempModel = new GroupStatisticsVM
                {
                    Average = (userskills.Count != 0) ? userskills.Average(x => x.SkillLevel).ToString() : "-1",
                    SkillName = skill.Skill,
                    SkillGoal = (skillGoal == null) ? -1 : skillGoal.SkillGoal

                };

                model.Add(tempModel);

            }
            ViewBag.LatestGoal = _context.SkillGoals.OrderByDescending(x => x.Date).Select(x=>x.Date).FirstOrDefault();
            //---------------end-------------//


            return View(model);
        }
    }
}
