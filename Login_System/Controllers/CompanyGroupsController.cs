using System.Linq;
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
using System.Security.Cryptography.X509Certificates;

namespace Login_System.Controllers
{

    public class CompanyGroupsController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        public CompanyGroupsController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CompanyGroups.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyGroup = await _context.CompanyGroups
                .FirstOrDefaultAsync(m => m.CompanyGroupId == id);
            if (companyGroup == null)
            {
                return NotFound();
            }

            var model = new CompanyGroups();
            List<Company> companies = new List<Company>();
            foreach(var comp in _context.CompanyGroupMembers.Where(x => x.CompanyGroupId == id).ToList())
            {
                Company company = _context.Company.FirstOrDefault(x => x.Id == comp.CompanyId);
                companies.Add(company);
            }
            model.companiesInGroups = companies; 



            ViewBag.CompanyGroupName = companyGroup.CompanyGroupName;

            return View(model);
        }
        public IActionResult Create()
        {
            var model = new CompanyGroups();

            foreach (var company in _context.Company)
            {
                model.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Id.ToString() });
            }
            foreach (var skill in _context.Skills)
            {
                model.SkillList.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Id.ToString() });
            }



            return View(model);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompanyGroupId,CompanyGroupName, Company, Skill")]  CompanyGroups data, int id)
        {
            var companyList = _context.Company;

            var company = (await _context.CompanyGroups.AddAsync(new CompanyGroups { CompanyGroupName = data.CompanyGroupName })).Entity;
            await _context.SaveChangesAsync();

            if (data.Company != null)
            {
                foreach (var companyid in data.Company)
                {
                    _context.Add(new CompanyGroupMember
                    {
                        CompanyGroupId = company.CompanyGroupId,
                        CompanyId = int.Parse(companyid)
                    });
                }
            }
            if (data.Skill != null)
            {
                foreach (var skillId in data.Skill)
                {
                    var skill = _context.Skills.FirstOrDefault(x => x.Id == int.Parse(skillId));

                    _context.CompanyGroupSkills.Add(new CompanyGroupSkill
                    {
                        SkillId = skill.Id,
                        CompanyGroupId = company.CompanyGroupId,
                        CompanyId = null,
                    });
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var model = new CompanyGroups();

            foreach (var company in _context.Company)
            {
                model.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Id.ToString() });
            }

            var companyGroup = await _context.CompanyGroups.FindAsync(id);
            if (companyGroup == null)
            {
                return NotFound();
            }
            var companies = _context.Company.Where(x=> x.CompanyGroupId == id).ToList();
            ViewBag.Companies = companies;
            model.CompanyGroupName = companyGroup.CompanyGroupName;
            model.CompanyGroupId = companyGroup.CompanyGroupId;
         
            return View(model);
        }
        [HttpPost]
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        public async Task<IActionResult> Edit(int id, [Bind("CompanyGroupId,CompanyGroupName, Company, oldCompanies")] CompanyGroups companyGroup)
        {
            if (companyGroup != null && id != companyGroup.CompanyGroupId) 
            {
                return NotFound();
            }
        
            if (ModelState.IsValid)
            {
                try
                {
                    List<Company> oldCompanyIds = _context.Company.Where(x => x.CompanyGroupId == id).ToList();
                    if (companyGroup.Company != null) 
                    {
                        foreach (var company in oldCompanyIds)
                        {
                            if (!companyGroup.Company.Contains(company.Id.ToString()))
                            {
                                company.CompanyGroupId = 0;
                                _context.Company.Update(company);
                            }
                        }
                    }

                    else
                    {
                        foreach (var company in oldCompanyIds)
                        {
                            company.CompanyGroupId = 0;
                            _context.Company.Update(company);
                        }
                    }
                    _context.Update(companyGroup);

                    if (companyGroup.Company != null) 
                    { 
                        foreach(var companyId in companyGroup.Company)
                        {
                            var company = _context.Company.FirstOrDefault(x=> x.Id == int.Parse(companyId));
                            company.CompanyGroupId = companyGroup.CompanyGroupId;
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) 
                {
                    if (!CompanyGroupExists(companyGroup.CompanyGroupId))
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
            return View(companyGroup);
            
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyGroup = await _context.CompanyGroups.FindAsync(id);
                
            if (companyGroup == null)
            {
                return NotFound();
            }

            var notEmpty = _context.Company.Where(x => x.CompanyGroupId == companyGroup.CompanyGroupId).ToList();

            if (notEmpty.Count < 1)
            {
                _context.CompanyGroups.Remove(companyGroup);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Delete successful" });
            }



            else 
            {
                return Json(new { success = false, message = "Cannot delete before all companies are deleted from company group." });
                
            }
            
        }

        //public IActionResult DeleteConfirmed(int id)
        //{

        //    return RedirectToAction(nameof(Index));
        //}

        private bool CompanyGroupExists(int id)
        {
            return _context.CompanyGroups.Any(e => e.CompanyGroupId == id);
        }
    }
}
