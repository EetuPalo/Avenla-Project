﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Login_System.Controllers
{

    public class CompaniesController : Controller
    {
        private readonly GeneralDataContext _context;
        private readonly IdentityDataContext _datacontext;
        private UserManager<AppUser> UserMgr { get; }

        public CompaniesController(GeneralDataContext context, UserManager<AppUser> userManager, IdentityDataContext datacontext)
        {
            _context = context;
            _datacontext = datacontext;
            UserMgr = userManager;
        }

        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Company.ToListAsync());
        }

        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Company
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            var model = new CompanyMembersVM();

            var list = new List<(int, string)>();
            foreach (var goals in _context.CompanyGoals.Where(x => x.CompanyID == id)) 
            {
                var skill = _context.Skills.FirstOrDefault(x => x.Id == goals.SkillID);
                list.Add((goals.CompanyGoal, skill.Skill));
            }

            model.ListofGoals = list;
            model.name = company.Name;
            model.Id = company.Id;
            model.Description = company.Description;

            var userList = new List<AppUser>();

            foreach (var user in _context.CompanyMembers.Where(x=> x.CompanyId == id))
            {
                var companyMember = _datacontext.Users.FirstOrDefault(x => x.Id == user.UserId);
                userList.Add(companyMember);
            }

            model.Users = userList;

            return View(model);
        }

        [Authorize(Roles = "Superadmin")]
        public IActionResult Create()
        {
            var model = new CompanyMembersVM();
            var userList = new List<AppUser>();

            // Populating CompanyMembersDropdown with users
            foreach (var users in UserMgr.Users)
            {
                model.userList.Add(new SelectListItem() { Text = users.FirstName + " " + users.LastName, Value = users.Id.ToString() });
            }

            return View(model);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Company company, int? id, CompanyMembersVM data)
        {


            if (id == null)
            {
                id = Convert.ToInt32(UserMgr.GetUserId(User));
            }
            //Finding the correct user
            var appUser = await UserMgr.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            AppUser user = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserId"] = id;



            if (ModelState.IsValid)
            {


                company = (await _context.Company.AddAsync(new Company { Name = data.CompanyName, Description = data.Description, CompanyGroupId = 0 })).Entity;
                await _context.SaveChangesAsync();


                if (data.SelectedUserIds != null)
                {
                    foreach (var userid in data.SelectedUserIds)
                    {
                        var companyMember = UserMgr.Users.FirstOrDefault(x => x.Id == userid);
                        var memberToAdd = new CompanyMember
                        {
                            CompanyId = company.Id,
                            UserId = companyMember.Id,
                            CompanyGroupAdmin = 0,

                        };
                        _context.CompanyMembers.Add(memberToAdd);
                    }
                }
                await _context.SaveChangesAsync();
                // return RedirectToAction(nameof(Create), "Companies", new { id = group.id, name = group.name });
                return RedirectToAction(nameof(Index));
            }
            return View(data);
        }

        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Company.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Company company)
        {
            if (company != null && id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }
        [HttpDelete]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Company.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            _context.Company.Remove(company);
            var users = _context.CompanyMembers.Where(x => x.CompanyId == id);
            foreach (var user in users)
            {
                _context.CompanyMembers.Remove(user);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Delete successful" });
        }


        [Authorize(Roles = "Superadmin")]
        private bool CompanyExists(int id)
        {
            return _context.Company.Any(e => e.Id == id);
        }
        
        [HttpGet]
        public async Task<IActionResult> CompanyGoals(int? cid)
        {
            if (cid == null)
            {
                return NotFound();
            }

            var company = await _context.Company.FindAsync(cid);
            var cgid = _context.CompanyGroupMembers.FirstOrDefault(x=> x.CompanyId == cid);
            var companyGroupSkills = _context.CompanyGroupSkills.Where(x => (x.CompanyGroupId == cgid.CompanyGroupId && x.CompanyId == null) || (x.CompanyGroupId == cgid.CompanyGroupId && x.CompanyId == cid)).ToList();

            var model = new CreateCompanyGoals();
            var skills = new List<Skills>();

            ViewBag.CompanyGoals = _context.CompanyGoals.Where(x => x.CompanyID == cid).ToList();

            foreach (var skill in companyGroupSkills)
            {
                skills.Add(_context.Skills.FirstOrDefault(x=> x.Id == skill.SkillId));
            }

            model.Skills = skills;
            model.CompanyID = (int)cid;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompanyGoals(int cid, [Bind("Id, CompanyGoal, SkillID, CompanyID")] CreateCompanyGoals model, int[] CompanyGoal)
        {
            if (model.CompanyID == 0)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var lista = _context.CompanyGoals.Where(x => x.CompanyID == model.CompanyID).ToList();
                int i = 0;

                try
                {
                    if (lista.Count() == 0)
                    {
                        foreach (var skill in model.SkillID)
                        {
                            if (!lista.Any(x => x.SkillID == skill))
                            {
                                var companyGoal = new CompanyGoals
                                {
                                    CompanyGoal = CompanyGoal[i],
                                    CompanyID = model.CompanyID,
                                    SkillID = skill
                                };

                                _context.Add(companyGoal);
                                i++;
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                    else 
                    {
                        foreach (var skill in model.SkillID)
                        {
                            var cGoal = lista.FirstOrDefault(x => x.SkillID == skill);
                            if (cGoal == null)
                            {
                                var companyGoal = new CompanyGoals
                                {
                                    CompanyGoal = CompanyGoal[i],
                                    CompanyID = model.CompanyID,
                                    SkillID = skill
                                };

                                _context.Add(companyGoal);
                                i++;
                            }
                            else 
                            {
                                cGoal.CompanyGoal = CompanyGoal[i];
                                _context.Update(cGoal);
                                i++;
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}
