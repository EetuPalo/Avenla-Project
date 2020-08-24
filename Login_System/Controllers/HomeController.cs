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

namespace Login_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> UserMgr;
        private readonly GeneralDataContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, GeneralDataContext context)
        {
            _logger = logger;
            UserMgr = userManager;
            _context = context;
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
         public async Task<IActionResult> SetActiveCompany(string company, string returnUrl)
         {

             Company Comp = _context.Company.FirstOrDefault(x => x.Id == int.Parse(company));
             await changeActiveCompany(Comp);
             return LocalRedirect(returnUrl);
         }

         public async Task<IdentityResult> changeActiveCompany(Company company)
         {
             var user = await UserMgr.FindByIdAsync(TempData["id"].ToString());
             user.Company = company.Id;
             return await UserMgr.UpdateAsync(user);
         }
    }
}
