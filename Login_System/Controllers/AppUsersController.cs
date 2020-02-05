using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Login_System.Controllers
{
    public class AppUsersController : Controller
    {
        private readonly IdentityDataContext _context;
        private UserManager<AppUser> UserMgr { get; }
        private SignInManager<AppUser> SignInMgr { get; }

        public AppUsersController(IdentityDataContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            UserMgr = userManager;
            SignInMgr = signInManager;
        }

        // GET: AppUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: AppUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // GET: AppUsers/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: AppUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(appUser);
        }

        // GET: AppUsers/Edit/5
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await UserMgr.FindByIdAsync(id.ToString());
            
            if (appUser == null)
            {
                return NotFound();
            }
            
            return View(appUser);
        }
        // POST: AppUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("FirstName, LastName, UserName, Email, PhoneNumber, Active")] AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                var compareUser = User.Identity.Name;
                var user = await UserMgr.FindByIdAsync(id.ToString());

                if (user.UserName == compareUser || User.IsInRole("Admin"))
                {                   
                    user.FirstName = appUser.FirstName;
                    user.LastName = appUser.LastName;
                    user.UserName = appUser.UserName;
                    user.Email = appUser.Email;
                    user.PhoneNumber = appUser.PhoneNumber;
                    user.Active = appUser.Active;

                    //This signs in with the new username
                    await SignInMgr.SignInAsync(user, false);
                    var result = await UserMgr.UpdateAsync(user);

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.Message = "You do not have the permission to edit this user!";
                    Console.WriteLine("You do not have the permission to edit this user!");
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(appUser);
        }

        // GET: AppUsers/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // POST: AppUsers/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appUser = await _context.Users.FindAsync(id);
            _context.Users.Remove(appUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppUserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
