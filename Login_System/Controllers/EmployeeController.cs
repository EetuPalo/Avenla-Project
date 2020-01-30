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
                    Active = dr["Active"] as string,
                    FirstName = dr["FirstName"] as string,
                    LastName = dr["LastName"] as string,
                    Email = dr["Email"] as string,
                    Telephone = dr["Telephone"] as string
                };
                items.Add(result);
            }
            empCon.Close();
            return items;
        }

        public IActionResult Index()
        {
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