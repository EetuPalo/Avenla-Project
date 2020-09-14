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
            return View(await _context.Company.ToListAsync());
        }

        public IActionResult Details(int? id)
        {

            return View();
        }
        public IActionResult Create()
        {

            return View();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        public IActionResult Create([Bind("CompanyGroupId,CompanyGroupName")] CompanyGroups company)
        {
            
            return RedirectToAction(nameof(Index));
        }

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
        public IActionResult Edit(int id, [Bind("CompanyGroupId,CompanyGroupName")] CompanyGroups company)
        {

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> Delete(int? id)
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

            return View(company);
        }
        
        public IActionResult DeleteConfirmed(int id)
        {

            return RedirectToAction(nameof(Index));
        }
    }
}
