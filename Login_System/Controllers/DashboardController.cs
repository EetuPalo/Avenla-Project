using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Login_System.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Resources;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.CompilerServices;

namespace Login_System.Controllers
{
    [Authorize(Roles = "User, Admin")]
    public class DashboardController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        
        public DashboardController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var model = new DashboardVM();

            var user = await UserMgr.GetUserAsync(HttpContext.User);
            ViewBag.CurrentCompany = user.Company;            
            ViewBag.CurrentUserLastName = user.FirstName;
            ViewBag.CurrentUserFirstName = user.LastName;
            ViewBag.CurrentUserEmail = user.Email;
            ViewBag.CurrentUserPhone = user.PhoneNumber;


            return View(model);
        }
    }
}