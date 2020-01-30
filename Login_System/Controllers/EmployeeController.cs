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

        public List<Employee> GetEmployee()
        {
            SqlConnection empCon = new SqlConnection(EmployeeConnection.ConString());
            string query = "SELECT * from dbo.Employee";
            SqlCommand cmd = new SqlCommand(query, empCon);
            empCon.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            
            List<Employee> items = new List<Employee>();
            foreach (var entry in dr)
            {
                Employee result = new Employee()
                {
                    Active = (bool)dr["Active"],
                    FirstName = dr["FirstName"] as string,
                    LastName = dr["LastName"] as string
                };
                items.Add(result);
            }
            empCon.Close();
            return items;
        }

        public IActionResult Index()
        {
            //var empList = GetEmployee();
            ////https://stackoverflow.com/questions/47796852/mvc-net-core-2-0-query-sql-server-database-and-return-results-in-grid-view
            ////return View();

            //var empVM = new List<EmployeeVM>();

            //foreach (Employee emp in empList)
            //{
            //    var personVM = new EmployeeVM()
            //    {
            //        LastName = emp.LastName,
            //        FirstName = emp.FirstName,
            //        Email = emp.Email,
            //        Telephone = emp.Telephone
            //    };
            //    empVM.Add(personVM);
            //}

            return View(GetEmployee());
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