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
using Login_System.ViewModels;

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

            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserId"] = id;
            TempData["UserFullName"] = tempUser.FirstName + " " + tempUser.LastName;

            return View(appUser);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("EMail, FirstName, LastName, PhoneNumber, Password, ConfirmPassword")] RegisterVM appUser)
        {
            if (ModelState.IsValid)
            {
                //This constructs the username from the users first and last names
                string userName = appUser.FirstName + appUser.LastName;

                //This is supposed to remove any special characters from the userName string
                byte[] tempBytes;
                tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(userName);
                string fixedUn = System.Text.Encoding.UTF8.GetString(tempBytes);

                AppUser user = await UserMgr.FindByNameAsync(fixedUn);
                if (user == null)
                {
                    try
                    {
                        user = new AppUser();
                        //we create a new user and set his credentials to the data received from the Register form.
                        user.UserName = fixedUn;
                        user.Email = appUser.EMail;
                        user.FirstName = appUser.FirstName;
                        user.LastName = appUser.LastName;
                        user.PhoneNumber = appUser.PhoneNumber;
                        //we then create a new user through usermanager
                        IdentityResult result;
                        IdentityResult roleResult;
                        result = await UserMgr.CreateAsync(user, appUser.Password);
                        roleResult = await UserMgr.AddToRoleAsync(user, "User");
                        TempData["CreateStatus"] = "User has been created!";
                    }
                    catch
                    {
                        Console.WriteLine("An error occured but the account may have still been created. Check the account list!");
                        TempData["CreateStatus"] = "An error occured but the account may have still been created. Check the account list!";
                    }
                   
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["CreateStatus"] = "Username taken!";
                    return RedirectToAction("Index");
                }
            }
            return View(appUser);

        }

        // GET: AppUsers/Edit/5
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.IsInRole("Admin") || UserMgr.GetUserId(User) == id.ToString())
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

                AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());

                TempData["UserId"] = id;
                TempData["UserFullName"] = tempUser.FirstName + " " + tempUser.LastName;
                return View(appUser);
            }
            return View();
        }
        // POST: AppUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
       //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("FirstName, LastName, Email, NewPassword, ConfirmNewPassword, PhoneNumber, Active")] AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                var compareUser = User.Identity.Name;
                var user = await UserMgr.FindByIdAsync(id.ToString());

                //This constructs the username from the users first and last names
                string userName = appUser.FirstName + appUser.LastName;

                //This is supposed to remove any special characters from the userName string
                byte[] tempBytes;
                tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(userName);
                string fixedUn = System.Text.Encoding.UTF8.GetString(tempBytes);

                //This is just an extra step to make sure the user is authorized to edit the account
                if (user.UserName == compareUser || User.IsInRole("Admin"))
                {
                    user.FirstName = appUser.FirstName;
                    user.LastName = appUser.LastName;
                    user.UserName = fixedUn;
                    user.Email = appUser.Email;
                    user.PhoneNumber = appUser.PhoneNumber;
                }

                if (appUser.NewPassword == null)
                {
                    try
                    {
                        var result = await UserMgr.UpdateAsync(user);
                        TempData["ActionResult"] = "User edited!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        Console.WriteLine("Updating user information failed!");
                        TempData["ActionResult"] = "User edit failed!";
                        return View(appUser);
                    }
                }
                else if (appUser.NewPassword != null)
                {
                    try
                    {
                        //This changes the password if the user has edited it
                        var hashResult = UserMgr.PasswordHasher.HashPassword(appUser, appUser.NewPassword);
                        var token = await UserMgr.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await UserMgr.ResetPasswordAsync(user, token, appUser.NewPassword);
                        var result = await UserMgr.UpdateAsync(user);

                        TempData["ActionResult"] = "User edited!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        Console.WriteLine("Updating user information with password failed!");
                        TempData["ActionResult"] = "User edit failed!";
                        return View(appUser);
                    }
                }                  
                else
                {
                    Console.WriteLine("You do not have the permission to edit this user!");
                    TempData["ActionResult"] = "User edit failed!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(appUser);
        }

        // GET: AppUsers/Delete/5
        //[Authorize(Roles = "Admin")]
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

            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());

            TempData["UserId"] = id;
            TempData["UserFullName"] = tempUser.FirstName + " " + tempUser.LastName;

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
            TempData["ActionResult"] = "User has been deleted!";
            return RedirectToAction(nameof(Index));
        }

        private bool AppUserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
