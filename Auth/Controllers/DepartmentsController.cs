using Auth.DAL;
using Auth.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Auth.Controllers
{
    public class DepartmentsController : Controller
    {
        public ActionResult Index()
        {
            // Only admins can manage departments
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                return View("AccessDenied");

            List<Department> departments = new List<Department>();

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string query = @"
                                SELECT d.Id, d.Name, d.HeadId, u.FullName AS HeadName
                                FROM Departments d
                                LEFT JOIN Users u ON d.HeadId = u.Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        departments.Add(new Department
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString(),
                            HeadId = reader["HeadId"] as int?,
                            HeadName = reader["HeadName"]?.ToString() ?? "No Head"
                        });
                    }
                }
            }

            return View(departments);
        }
    }
}