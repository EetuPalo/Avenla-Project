using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Login_System.Controllers
{
    public class AdvancedSearchController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}