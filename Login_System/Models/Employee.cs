using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class Employee
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Telephone { get; set; }

        public string Email { get; set; }
        public bool Active { get; set; }

        public int SaveEmployee()
        {
            SqlConnection empCon = new SqlConnection(EmployeeConnection.ConString());
            string query = "INSERT INTO dbo.Employee(FirstName, LastName, Telephone, Email, Active) values ('" + FirstName + "','" + LastName + "','" + Telephone + "','" + Email + "','" + Active + "')";
            SqlCommand cmd = new SqlCommand(query, empCon);
            empCon.Open();
            int i = cmd.ExecuteNonQuery();
            empCon.Close();
            return i;
        }

        
    }
}
