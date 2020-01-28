using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Login_System.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> UserMgr { get; }
        private SignInManager<AppUser> SignInMgr { get; }

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            UserMgr = userManager;
            SignInMgr = signInManager;
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
        public async Task<IActionResult> Register([Bind("UserName, EMail, FirstName, LastName, Password, ConfirmPassword")] RegisterVM newUser)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await UserMgr.FindByNameAsync(newUser.UserName);
                if (user == null)
                {
                    

                    user = new AppUser();

                    user.UserName = newUser.UserName;
                    user.Email = newUser.EMail;
                    user.FirstName = newUser.FirstName;
                    user.LastName = newUser.LastName;

                    IdentityResult result;
                    result = await UserMgr.CreateAsync(user, newUser.Password);
                    ViewBag.Message("User has been created!");
                    return View("Index");
                }
                else
                {
                    ViewBag.Message = "Username taken!";
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
            var result = await SignInMgr.PasswordSignInAsync(user.UserName, user.Password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Result = "result is: " + result.ToString();
            }
            return View();
        }
    }
}