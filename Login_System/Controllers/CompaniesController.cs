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
    
    public class CompaniesController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        public CompaniesController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
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
                .FirstOrDefaultAsync(m => m.id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [Authorize(Roles = "Superadmin")]
        public IActionResult Create()
        {
            var model = new CompanyMembersVM();
            var userList = new List<AppUser>();

            // Populating CompanyMembersDropdown with users
            foreach (var users in UserMgr.Users)
            {
                model.userList.Add(new SelectListItem() { Text = users.UserName, Value = users.Id.ToString() });
            }

            return View(model);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> Create([Bind("id,name,Description")] Company company, int? id)
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
                _context.Add(company);

                var companyMembers = new CompanyMembersVM
                {
                    CompanyId = company.id,
                    CompanyName = company.name,
                    UserId = user.Id,
                    UserName = user.UserName
                };


                await _context.SaveChangesAsync();



                // return RedirectToAction(nameof(Create), "Companies", new { id = group.id, name = group.name });
                return RedirectToAction(nameof(Index));
            }
            return View(company);
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
        public async Task<IActionResult> Edit(int id, [Bind("id,name,Description")] Company company)
        {
            if (id != company.id)
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
                    if (!CompanyExists(company.id))
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

        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Company
                .FirstOrDefaultAsync(m => m.id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Company.FindAsync(id);
            _context.Company.Remove(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Superadmin")]
        private bool CompanyExists(int id)
        {
            return _context.Company.Any(e => e.id == id);
        }
    }
}
