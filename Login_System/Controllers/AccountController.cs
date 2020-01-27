using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Login_System.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> UserMgr { get; }
        private SignInManager<AppUser> SignInMgr { get; }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Register(User newUser)
        {
            try
            {
                ViewBag.Message = "User already registered";
                AppUser user = await UserMgr.FindByNameAsync(newUser.userName);
                if (user == null)
                {
                    user = new AppUser();

                    //These two are inherited from IdentityUser
                    user.UserName = newUser.userName;
                    user.Email = newUser.eMail;

                    //These two were created in AppUser
                    user.FirstName = newUser.firstName;
                    user.LastName = newUser.lastName;

                    IdentityResult result;

                    result = await UserMgr.CreateAsync(user, newUser.password);

                    if (result.Succeeded)
                    {
                        ViewBag.Message = "User created!";
                    }
                    else
                    {
                        //Shows what went wrong
                        ViewBag.Message = result.Errors;
                    }


                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }           

            //Default is to return the view with the same name as the the action method which calls it
            return View();
        }

        public async Task<IActionResult> Login(User user)
        {
            var result = await SignInMgr.PasswordSignInAsync(user.userName, user.password, false, false);
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