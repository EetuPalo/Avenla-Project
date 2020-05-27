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

namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GroupsController : Controller
    {
        private readonly GeneralDataContext _context;

        public GroupsController(GeneralDataContext context)
        {
            _context = context;
        }

        // GET: Groups
        public async Task<IActionResult> Index(string searchString)
        {
            var groups = from g in _context.Group select g;
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
        public IActionResult Create()
        {
            var model = new GroupVM();
            foreach (var company in _context.Company)
            {
                model.CompanyList.Add(new SelectListItem() { Text = company.name, Value = company.name });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,name,company")] Group @group)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@group);
                await _context.SaveChangesAsync();
                //Some data to build the "guide"
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GroupCreated;
                TempData["ActionPhase"] = "[2/3]";
                TempData["Source"] = "create";
                TempData["GroupName"] = group.name;
                TempData["GroupId"] = group.id;
                TempData["GroupCompany"] = group.company;
                return RedirectToAction(nameof(Create), "SkillGoals", new { name = group.name });
            }
            TempData["ActionResult"] = Resources.ActionMessages.ActionResult_Error;           
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
        public async Task<IActionResult> Delete(int? id, string? source)
        {
            if (source != null)
            {
                TempData["Source"] = source;
            }

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @group = await _context.Group.FindAsync(id);

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
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(int id)
        {
            return _context.Group.Any(e => e.id == id);
        }

        public async Task<IActionResult> Statistics(int id)
        {
            var model = new List<GroupStatisticsVM>();

            Group tempGroup = await _context.Group.FindAsync(id);
            var memberList = _context.GroupMembers.Where(g => g.GroupID == id).ToList();
            var goalList = _context.SkillGoals.Where(g => g.GroupName == tempGroup.name).ToList();
            var userSkills = _context.UserSkills.ToList();           
            var userSkillList = new Dictionary<int, List<DateTime>>();
            var maxDateList = new Dictionary<int, DateTime>();
            var skillAvgList = new Dictionary<string, List<int>>();           

            //Messages that are shown at the top of the page
            ViewBag.GroupName = tempGroup.name;
            //General data about the group
            ViewBag.MemberCount = memberList.Count(m => m.GroupID == id);

            var dateList = new List<DateTime>();
            foreach (var goal in goalList)
            {
                if (!dateList.Contains(goal.Date))
                {
                    dateList.Add(goal.Date);
                }
            }
            DateTime? latestDate = null;
            if (dateList.Count() > 0)
            {
                latestDate = dateList.Max();
                ViewBag.LatestGoal = dateList.Max().ToString("dd.MM.yyyy");
            }
            else
            {
                ViewBag.LatestGoal = Resources.ActionMessages.Stats_Avg_NoData;
            }

            TempData["GroupID"] = id;
            TempData["GroupName"] = tempGroup.name;

            //-------------AVERAGE------------
            foreach (var member in memberList)
            {
                //Empties the list at the start of the loop
                var userDateList = new List<DateTime>();
                foreach (var userSkill in userSkills.Where(x => x.UserID == member.UserID))
                {
                    if (!userDateList.Contains(userSkill.Date))
                    {
                        userDateList.Add(userSkill.Date);
                    }
                }
                userSkillList.Add(member.UserID, userDateList);
            }
            
            foreach (var user in userSkillList.Where(x => x.Value != null))
            {
                try
                {
                    maxDateList.Add(user.Key, user.Value.Max());
                }
                catch
                {
                    Console.WriteLine("No skills for this user.");
                }
            }
            foreach (var skill in _context.Skills.ToList())
            {
                var tempSkills = new List<int>();
                foreach (var uskill in userSkills.Where(x => x.SkillName == skill.Skill))
                {                    
                    foreach (var entry in maxDateList.Where(x => (x.Key == uskill.UserID) && (x.Value == uskill.Date)))
                    {
                        tempSkills.Add(uskill.SkillLevel);
                    }
                }
                skillAvgList.Add(skill.Skill, tempSkills);
            }          

            foreach (var goal in goalList.Where(x => (x.GroupName == tempGroup.name) && (x.Date == latestDate)))
            {
                var tempModel = new GroupStatisticsVM
                {
                    SkillName = goal.SkillName,
                    SkillGoal = goal.SkillGoal
                };

                foreach (var skill in skillAvgList.Where(x => x.Key == tempModel.SkillName))
                {
                    try
                    {
                        var avrg = skill.Value.Average();
                        tempModel.Average = String.Format("{0:0.00}", avrg);
                    }
                    catch
                    {
                        tempModel.Average = Resources.ActionMessages.Stats_Avg_NoData;
                    }
                }
                model.Add(tempModel);
            }
            return View(model);
        }
    }
}
