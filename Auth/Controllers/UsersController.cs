using Auth.DAL;
using Auth.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Auth.Controllers
{
    public class UsersController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return View("AccessDenied");
            }

            List<User> users = new List<User>();

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string query = "SELECT * FROM Users";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = (int)reader["Id"],
                            FullName = reader["FullName"].ToString(),
                            Email = reader["Email"].ToString(),
                            Role = reader["Role"].ToString()
                        });
                    }
                }
            }

            return View(users);
        }




        [HttpPost]
        public ActionResult ChangeRole(int userId, string role)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return View("AccessDenied");
            }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string query = "UPDATE Users SET Role=@Role WHERE Id=@Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@Id", userId);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Search(string from, string to)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                return View("AccessDenied");

            // Server-side validation
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                ViewBag.ErrorMessage = "Please select both From and To dates.";
                return View("Index", new List<User>()); // return empty list
            }

            List<User> users = new List<User>();

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("GetUsersByPeriod", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@FromDate", DateTime.Parse(from));
                    cmd.Parameters.AddWithValue("@ToDate", DateTime.Parse(to));

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = (int)reader["Id"],
                            FullName = reader["FullName"].ToString(),
                            Email = reader["Email"].ToString(),
                            Role = reader["Role"].ToString()
                        });
                    }
                }
            }
            // Pass selected period to view
            ViewBag.FromDate = from;
            ViewBag.ToDate = to;
            return View("Index", users);
        }
    }
}