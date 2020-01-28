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
        public async Task<IActionResult> Register([Bind("UserName, EMail, FirstName, LastName, Password")] RegisterVM newUser)
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
                    ViewBag.Message("User created!");
                    return RedirectToAction("index");
                }
                else
                {
                    ViewBag.Message = "Username taken!";
                    return RedirectToAction("index");
                }
            }
            return View(newUser);

            
        }


        //[HttpPost]
        //public async Task<IActionResult> Register(User newUser)
        //{
        //    //ViewBag.Message = "User already registered";

        //    if (!ModelState.IsValid)
        //    {
        //        return View(newUser);
        //    }
        //    else
        //    {
        //        AppUser user = await UserMgr.FindByNameAsync(newUser.userName);

        //        if (user == null)
        //        {
        //            user = new AppUser();

        //            //These two are inherited from IdentityUser
        //            user.UserName = newUser.userName;
        //            user.Email = newUser.eMail;

        //            //These two were created in AppUser
        //            user.FirstName = newUser.firstName;
        //            user.LastName = newUser.lastName;

        //            IdentityResult result;

        //            result = await UserMgr.CreateAsync(user, newUser.password);

        //            /*
        //            if (result.Succeeded)
        //            {
        //                ViewBag.Message = "User created!";
        //            }
        //            else
        //            {
        //                //Shows what went wrong
        //                //ViewBag.Message = result.Errors;
        //            }
        //            */
        //        }
        //        //Default is to return the view with the same name as the the action method which calls it
        //        return RedirectToAction("Index");
        //    }
        //}
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