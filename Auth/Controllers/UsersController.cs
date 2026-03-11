using Auth.DAL;
using Auth.Models;
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
                            Username = reader["Username"].ToString(),
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
    }
}