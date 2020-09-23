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
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Admin"))
            {
                var currentUser = await UserMgr.GetUserAsync(HttpContext.User);
                TempData["CompanyID"] = currentUser.Company;
            }
            var model = new GroupVM();
            foreach (var company in _context.Company)
            {
                model.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Id.ToString()});
            }
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,name,CompanyId")] Group @group)
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
                //Some data to build the "guide"
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GroupCreated;
                TempData["ActionPhase"] = "[2/3]";
                TempData["Source"] = "create";
                TempData["GroupName"] = group.name;
                TempData["GroupId"] = group.id;
                TempData["GroupCompany"] = group.company;
                return RedirectToAction(nameof(AddSkills), "Groups", new { id = addGroup.id, name = group.name });
            }
            TempData["ActionResult"] = Resources.ActionMessages.ActionResult_Error;           
            return View(@group);
        }
        [HttpGet]
        public IActionResult AddSkills(int id, string name)
        {
            var model = new CreateSkillGoalsVM();
            var skillsList = new List<Skills>();
            var listModel = new List<SkillGoals>();
            DateTime date = DateTime.Now;
            //int dictKey = 0;
            //model.SkillCounter = 0;
            int groupId = _context.Group.FirstOrDefault(x => x.id == id).id;
            TempData["id"] = id;
            foreach (var skill in _context.Skills)
            {
                skillsList.Add(skill);
                var tempModel = new SkillGoals
                {
                    SkillId = skill.Id,
                    SkillName = skill.Skill,
                    GroupName = name,
                    GroupId = groupId
                };

                listModel.Add(tempModel);
                //dictKey++;
                //model.SkillCounter++;

            }
            model.GroupName = name;
            model.Groupid = id;
            model.GroupSkills = skillsList;
            model.SkillGoals = listModel;
            model.Skills = _context.Skills.Select(x => new SelectListItem
            {
                Value = x.Skill,
                Text = x.Skill
            });
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSkills(string source, [Bind("SkillGoal")] CreateSkillGoalsVM goals, string[] Skill, string GroupName, int Groupid, string skillName)
        {

            var x = goals.Groupid;
            //var groupName = _context.Group.FirstOrDefaultAsync(x => x.id == );
            foreach (var skill in Skill)
            {
                var skillFromTable = await _context.Skills.FirstOrDefaultAsync(x => x.Skill == skill);
                var skillGoal = new SkillGoals
                {
                    //SkillName = skill.Skill,
                    SkillGoal = -1,
                    Date = DateTime.Now,
                    SkillName = skillFromTable.Skill,
                    SkillId = skillFromTable.Id,
                    GroupName = GroupName,
                    GroupId = Groupid
                };
                _context.Add(skillGoal);
            }
            await _context.SaveChangesAsync();


            /**/

            if (source == "create")
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GoalSetAddUser;
                TempData["ActionPhase"] = "[3/3]";


                return RedirectToAction(nameof(Create), "GroupMembers", new { source = "create", id = Groupid, group = GroupName });
            }
            return RedirectToAction(nameof(Index), new { id = Groupid });

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
                foreach (var item in _context.UserSkills.Where(x => (x.SkillId == skill.Id)).OrderByDescending(x => x.Date))
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
