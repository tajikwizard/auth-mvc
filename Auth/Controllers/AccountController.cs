using Auth.DAL;
using Auth.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Auth.Controllers
{
    public class AccountController : Controller {



        public ActionResult TestConnection()


        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                }
                return Content("Connected to SQL Server successfully!");
            }
            catch (Exception ex)
            {
                return Content("Connection failed: " + ex.Message);
            }
        }



        // GET: Register Page
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register Form
        [HttpPost]
        public ActionResult Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("registerUser", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //cmd.Parameters.AddWithValue("@FullName", user.FullName.Trim());
                    //cmd.Parameters.AddWithValue("@Username", user.Username.Trim());
                    //cmd.Parameters.AddWithValue("@Email", user.Email.Trim());
                    //cmd.Parameters.AddWithValue("@Password", user.Password.Trim());
                    //cmd.Parameters.AddWithValue("@Role", "user");


                    cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 255).Value = user.FullName.Trim();
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 255).Value = user.Username.Trim();
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = user.Email.Trim();
                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = user.Password.Trim();
                    cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = user.Role.Trim();


                    // Output parameter for status
                    SqlParameter statusParam = new SqlParameter("@Status", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(statusParam);

                    cmd.ExecuteNonQuery();

                    int status = (int)statusParam.Value;
                    if (status == 0)
                    {
                        // Email already exists
                        ModelState.AddModelError("", "Ин почтаи электронӣ аллакай вуҷуд дорад.");
                        return View(user);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Хато рух дод: " + ex.Message);
                return View(user);
            }

            return RedirectToAction("Login");
        }



        // GET: Login Page
        public ActionResult Login()
        {
                return View();
        }

        // POST: Login Page
        [HttpPost]
        public ActionResult Login(string Email, string Password)
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("loginUser", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    //cmd.Parameters.AddWithValue("@Email", Email.Trim());
                    //cmd.Parameters.AddWithValue("@Password", Password);

                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = Email.Trim();
                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = Password.Trim();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];
                            // Create session
                            Session["UserId"] = row["Id"];
                            Session["Username"] = row["Username"];
                            Session["Role"] = row["Role"];

                            return RedirectToAction("Index", "Dashboard");
                        }
                        else
                        {
                            ViewBag.Error = "Почтаи электронӣ ё пароли нодуруст";
                            return View();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Хато: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {

            if (Session["UserId"] == null)
            {
                TempData["Error"] = "Шумо барои дастрасӣ ба ин саҳифа иҷозат надоред. Лутфан ворид шавед.";
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection form)
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login", "Account");

            string username = Session["Username"].ToString();
            string current = form["CurrentPassword"];
            string newPass = form["NewPassword"];

            if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(newPass))
            {
                ViewBag.ErrorMessage = "Fields cannot be empty!";
                return View();
            }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Verify current password
                string checkQuery = "SELECT Password FROM Users WHERE Username=@Username";
                using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    var dbPassword = cmd.ExecuteScalar()?.ToString();

                    if (dbPassword != current)
                    {
                        ViewBag.ErrorMessage = "Current password is incorrect!";
                        return View();
                    }
                }

                // Update password
                string updateQuery = "UPDATE Users SET Password=@Password WHERE Username=@Username";
                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Password", newPass);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.ExecuteNonQuery();
                }
            }

            ViewBag.SuccessMessage = "Password changed successfully!";
            return View();
        }



        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }


    }


 }
