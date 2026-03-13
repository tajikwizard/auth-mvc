using Auth.DAL;
using Auth.Models;
using DevExpress.CodeParser;
using Microsoft.Ajax.Utilities;
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

                    cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 255).Value = user.FullName.Trim();
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = user.Email.Trim();
                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = user.Password.Trim();
                    cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = user.Role.Trim();


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

                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = Email.Trim();
                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = Password.Trim();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable userTable = new DataTable();
                        adapter.Fill(userTable);
                        if (userTable.Rows.Count > 0)
                        {
                            DataRow row = userTable.Rows[0];
                            // Create session
                            Session["UserId"] = row["Id"];
                            Session["Email"] = row["Email"];
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

        //GET: Change Password Page
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

        //POST: Change Password Page
        [HttpPost]
        public ActionResult ChangePassword(FormCollection form)
        {
            if (Session["Email"] == null)
                return RedirectToAction("Login", "Account");

            string email = Session["Email"].ToString();
            string current = form["CurrentPassword"];
            string newPass = form["NewPassword"];

            if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(newPass))
            {
                ViewBag.ErrorMessage = "Майдонҳо холӣ буда наметавонанд.!";
                return View();
            }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Verify current password
                string checkQuery = "SELECT Password FROM Users WHERE Email=@Email";
                using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    //The null - conditional operator prevents errors if no record is returned
                    var dbPassword = cmd.ExecuteScalar()?.ToString();

                    if (dbPassword != current)
                    {
                        ViewBag.ErrorMessage = "Рамз нодуруст аст!";
                        return View();
                    }
                }

                // Update password
                string updateQuery = "UPDATE Users SET Password=@Password WHERE Email=@Email";
                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = newPass.Trim();
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email.Trim();
                    cmd.ExecuteNonQuery();
                }
            }

            ViewBag.SuccessMessage = "Парол бомуваффақият иваз карда шуд!";
            return View();
        }

        // GET: Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

    }
 }
