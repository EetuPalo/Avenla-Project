using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;
using Login_System.Helpers;
using System.Web;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Login_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> UserMgr;
        private readonly GeneralDataContext _context;
        private readonly IdentityDataContext _identity;
        private SignInManager<AppUser> SignInMgr { get; }

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, GeneralDataContext context, IdentityDataContext identity, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            UserMgr = userManager;
            _context = context;
            _identity = identity;
            SignInMgr = signInManager;
        }

        public IActionResult Index()
        {
            ViewBag.UserID = UserMgr.GetUserId(User);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    //IsEssential = true,  //critical settings to apply new culture 
                    //Path = "/",
                    //HttpOnly = false,
                }
            );
            return LocalRedirect(returnUrl);
        }

         [HttpPost]
         public async Task<IActionResult> SetActiveCompany(string company, string returnUrl, int id)
         {
            //change user role as well as active company
            var user = await UserMgr.FindByIdAsync(id.ToString());
            //get the info of company that the user is changing to 
            Company Comp = _context.Company.FirstOrDefault(x => x.Id == int.Parse(company));
            //list users roles in companies
            var userRoles = _context.CompanyMembers.Where(x=> x.UserId == id).ToList();
            //get role id:s used in remove and set in userroles table
            var oldRoleId = userRoles.FirstOrDefault(x => x.CompanyId == user.Company).CompanyRole;
            var newRoleId = userRoles.FirstOrDefault(x => x.CompanyId == int.Parse(company)).CompanyRole;
            //get roles
            var oldRole = _identity.Roles.FirstOrDefault(x => x.Id == oldRoleId);
            var role = _identity.Roles.FirstOrDefault(x => x.Id == newRoleId);
            IdentityResult result =  await UserMgr.RemoveFromRoleAsync(user,oldRole.Name);
     
            await changeActiveCompany(Comp);
            await UserMgr.AddToRoleAsync(user, role.Name);
            await SignInMgr.SignOutAsync();
            await SignInMgr.SignInAsync(user, false, default);

            return RedirectToAction("Index","Dashboard");
         }

         public async Task<IdentityResult> changeActiveCompany(Company company)
         {
             var user = await UserMgr.FindByIdAsync(TempData["id"].ToString());
             user.Company = company.Id;
             return await UserMgr.UpdateAsync(user);
         }
    }
}
