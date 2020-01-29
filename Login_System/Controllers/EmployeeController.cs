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
        private SqlConnection connection;
        
        public IActionResult Index(Employee emp)
        {
            var empList = emp.GetEmployee();
            //https://stackoverflow.com/questions/47796852/mvc-net-core-2-0-query-sql-server-database-and-return-results-in-grid-view
            //return View();
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
        //[HttpGet]
        //public IActionResult Employee()
        //{
        //    //string constr = EmployeeConnection.ConString();
        //    //string sql = "SELECT * FROM Employee";
        //    //SqlCommand cmd = new SqlCommand(sql, connection);

        //    //var model = new List<Employee>();
        //    //using (SqlConnection connection = new SqlConnection(constr))
        //    //{
        //    //    connection.Open();
        //    //    SqlDataReader reader = cmd.ExecuteReader();
        //    //    while (reader.Read())
        //    //    {
        //    //        var employee = new Employee();
        //    //        employee.Active = reader;
        //    //        employee.FirstName = reader["FirstName"];
        //    //        model.Add(employee);
        //    //    }
        //    //}
        //    //return View(model);
        //}
    }
}