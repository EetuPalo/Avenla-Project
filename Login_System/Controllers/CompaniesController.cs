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
                .FirstOrDefaultAsync(m => m.Id == id);
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
                model.userList.Add(new SelectListItem() { Text = users.FirstName + " "+ users.LastName, Value = users.Id.ToString() });
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
                //_context.Add(data);

                //var companyMembers = new CompanyMembersVM
                //{
                //    CompanyId = data.Id,
                //    CompanyName = data.CompanyName,
                //    UserId = user.Id,
                //    UserName = user.UserName
                //};

                company = (await _context.Company.AddAsync(new Company { Name = data.CompanyName, Description = data.Description})).Entity;
                await _context.SaveChangesAsync();


                if (data.SelectedUserIds != null) { 
                    foreach(var userid in data.SelectedUserIds)
                    {
                        var companyMember = UserMgr.Users.FirstOrDefault(x=> x.Id == userid);
                        var memberToAdd = new CompanyMember
                        {
                            CompanyId = company.Id,
                            UserId = companyMember.Id,
         
                        };
                        _context.CompanyMembers .Add(memberToAdd);
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
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Company.FindAsync(id);
            _context.Company.Remove(company);
           

            var users = _context.CompanyMembers.Where(x => x.CompanyId == id);
            foreach(var user in users)
            {
                _context.CompanyMembers.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Superadmin")]
        private bool CompanyExists(int id)
        {
            return _context.Company.Any(e => e.Id == id);
        }
    }
}
