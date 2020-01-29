using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Login_System.Models;
using Login_System.ViewModels;
using Microsoft.Data.SqlClient;

namespace Login_System.Controllers
{
    public class EmployeeController : Controller
    {
        private SqlConnection empCon = new SqlConnection();

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        
        [HttpPost]
        public IActionResult Add(Employee emp)
        {
            int result = emp.SaveEmployee();
            if (result>0)
            {
                ViewBag.Message = "New employee added!";
            }
            else
            {
                ViewBag.Message = "Something went wrong!";
            }

            return View("Index");
        }
        
    }
}