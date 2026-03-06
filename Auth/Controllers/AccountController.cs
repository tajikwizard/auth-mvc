using Auth.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Auth.DAL;
using System.Reflection;

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

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email";

                using (SqlCommand cmd = new SqlCommand(checkEmailQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email.Trim());

                    int count = (int)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        ModelState.AddModelError("", "Ин почтаи электронӣ аллакай вуҷуд дорад..");
                        return View(user);
                    }
                }

                string insertQuery = @"
                    INSERT INTO Users (FullName, Username, Email, Password, Role)
                    VALUES (@FullName, @Username, @Email, @Password, @Role)";


                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", user.FullName.Trim());
                    cmd.Parameters.AddWithValue("@Username", user.Username.Trim());
                    cmd.Parameters.AddWithValue("@Email", user.Email.Trim());

                    // Hash the password (simple example)
                    cmd.Parameters.AddWithValue("@Password", user.Password);

                    cmd.Parameters.AddWithValue("@Role", user.Role);

                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Success"] = "Registration successful! You can now login.";
            return RedirectToAction("Login");
        }
    
            // GET: Login Page
            public ActionResult Login()
            {
                return View();
            }

        [HttpPost]
        public ActionResult Login(string Email, string Password)
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string query = "SELECT * FROM Users WHERE Email=@Email AND Password=@Password";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", Email.Trim());
                        cmd.Parameters.AddWithValue("@Password", Password);

                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            // Create session
                            Session["UserId"] = reader["Id"];
                            Session["Username"] = reader["Username"];
                            Session["Role"] = reader["Role"];

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
                ViewBag.Error = ex.Message;
                return View();
            }
        }



        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }


    }


 }
