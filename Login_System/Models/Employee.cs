using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class Employee
    {
        [Required]
        [DisplayName("First Name")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }
        [Required]
        [DisplayName("Last Name")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        public string Telephone { get; set; }

        [DisplayName("E-Mail")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string Active { get; set; }

        public int Id { get; set; }

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

        public int DeleteEmployee()
    }
}
