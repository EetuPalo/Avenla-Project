using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Login_System.Models;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Login_System.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> UserMgr { get; }
        private SignInManager<AppUser> SignInMgr { get; }

        private readonly IdentityDataContext _context;
        private readonly RoleManager<AppRole> roleMgr;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IdentityDataContext context, RoleManager<AppRole> roleManager)
        {
            UserMgr = userManager;
            SignInMgr = signInManager;
            _context = context;
            roleMgr = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([Bind("EMail, FirstName, LastName, PhoneNumber, Password, ConfirmPassword")] RegisterVM newUser)
        {
            if (ModelState.IsValid)
            {
                //This constructs the username from the users first and last names
                string userName = newUser.FirstName + newUser.LastName;
                var k = 1;
                var veryTempUser = await UserMgr.FindByNameAsync(userName);
                while (veryTempUser != null)
                {
                    userName = userName + k;
                    veryTempUser = await UserMgr.FindByNameAsync(userName);
                    k++;
                }

                //This is supposed to remove any special characters from the userName string
                byte[] tempBytes;
                tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(userName);
                string fixedUn = Encoding.UTF8.GetString(tempBytes);
                fixedUn = RemoveSpecialCharacters(fixedUn);

                AppUser user = await UserMgr.FindByNameAsync(fixedUn);
                if (user == null)
                {                    
                    user = new AppUser();
                    //we create a new user and set his credentials to the data received from the Register form.
                    user.UserName = fixedUn;
                    user.Email = newUser.EMail;
                    user.FirstName = newUser.FirstName;
                    user.LastName = newUser.LastName;
                    user.PhoneNumber = newUser.PhoneNumber;
                    user.EmpStatus = "Active";
                    //we then create a new user through usermanager
                    IdentityResult result;
                    IdentityResult roleResult;
                    result = await UserMgr.CreateAsync(user, newUser.Password);

                    //This is only for the case that DB roles is empty
                    if (await roleMgr.RoleExistsAsync("User"))
                    {
                        roleResult = await UserMgr.AddToRoleAsync(user, "User");
                    }
                    else
                    {
                        AppRole userRole = new AppRole
                        {
                            Name = "User"
                        };
                        await roleMgr.CreateAsync(userRole);
                        await UserMgr.AddToRoleAsync(user, "User");
                    }
                    if (!await roleMgr.RoleExistsAsync("Admin"))
                    {
                        AppRole adminRole = new AppRole
                        {
                            Name = "Admin"
                        };
                        await roleMgr.CreateAsync(adminRole);
                        await UserMgr.AddToRoleAsync(user, "Admin");
                    }

                    ViewBag.Message = Resources.ActionMessages.ActionResult_UserCreated;
                    TempData["UserFullNames"] = user.FirstName + " " + user.LastName;
                    return View("Index");
                }
                else
                {
                    ViewBag.Message = Resources.ActionMessages.ActionResult_UserTaken;
                    return View("Index");
                }
            }
            return View(newUser);

            
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM user)
        {
            var tempUser = UserMgr.Users.FirstOrDefault(x => x.Email == user.Email);
            if (tempUser != null)
            {
                var result = await SignInMgr.PasswordSignInAsync(tempUser.UserName, user.Password, false, false);
                if (result.Succeeded)
                {
                    /* After User logs in, that user's "Active" field's value is changed to 'Active' */
                    var appUser = _context.Users.FirstOrDefault(acc => acc.UserName == tempUser.UserName);//find the user in the db
                                                                                                          //appUser.Active = "Active";//set the value to active
                    _context.Users.Attach(appUser);//attach to the user object
                    _context.Entry(appUser).Property(x => x.Active).IsModified = true;//tell the db context method that the property vlaue has changed
                    _context.SaveChanges();//save changes to the DB

                    //Constructs a string from users first and last names to be shown in loginpartial
                    TempData["UserFullNames"] = appUser.FirstName + " " + appUser.LastName;
                    //Sends the userID in viewbag to the view
                    ViewBag.UserID = appUser.Id;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Result = result.ToString();
                }               
            }
            return View(user);
        }        

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await SignInMgr.SignOutAsync();
            /* Basically the same as in the Login method but we find the current user that is logging out and set his status to 'Inactive'*/

            try
            {
                var appUser = _context.Users.FirstOrDefault(acc => acc.UserName == SignInMgr.UserManager.GetUserName(User));
                appUser.Active = "Inactive";
                _context.Users.Attach(appUser);
                _context.Entry(appUser).Property(x => x.Active).IsModified = true;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("Index", "Home");
        }
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}