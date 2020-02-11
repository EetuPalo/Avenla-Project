using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class UserSkills
    {
        public int Id { get; set; }

        public int UserID { get; set; }

        public string SkillName { get; set; }

        public int SkillLevel { get; set; }

        public DateTime Date { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Skill { get; set; }

        public string AdminEval { get; set; }

        SqlDataReader dr;
        public string GetUserName(int? id)
        {
            if (id != null)
            {
                string getUser = "SELECT UserName FROM [AspNetUsers] WHERE Id=" + id + ";";
                SqlConnection con = new SqlConnection("EmployeeConnection");
                string userName = null;
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(getUser, con);
                    dr = cmd.ExecuteReader();

                    userName = dr["UserName"].ToString();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    con.Close();
                }

                if (userName != null)
                {
                    return userName;
                }
                else
                {
                    Console.WriteLine("No user found!");
                    return userName;
                }
            }

            return null;

        }

    }
}
