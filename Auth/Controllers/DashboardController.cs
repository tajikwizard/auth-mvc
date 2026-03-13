using Auth.DAL;
using Auth.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Auth.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            // Check session
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "Шумо барои дастрасӣ ба ин саҳифа иҷозат надоред. Лутфан ворид шавед.";
                return RedirectToAction("Login", "Account");
            }
            List<Image> images = new List<Image>();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Images ORDER BY UploadedAt DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        images.Add(new Image
                        {
                            Id = (int)reader["Id"],
                            FileName = reader["FileName"].ToString(),
                            FilePath = reader["FilePath"].ToString(),
                            Uploader = reader["Uploader"].ToString(),
                            UploadedAt = (DateTime)reader["UploadedAt"]
                        });
                    }
                }
            }
            return View(images);
        }


        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            if (file == null || file.ContentLength == 0)
            {
                TempData["Error"] = "Ягон файл интихоб нашудааст.";
                return RedirectToAction("Index");
            }

            // Validate file type
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (Array.IndexOf(allowedExtensions, ext) < 0)
            {
                TempData["Error"] = "Формати файли нодуруст. Танҳо JPG, PNG, GIF иҷозат дода мешавад.";
                return RedirectToAction("Index");
            }

            // Validate file size (max 5 MB)
            if (file.ContentLength > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Файл хеле калон. Андозаи максималӣ 5 МБ аст.";
                return RedirectToAction("Index");
            }

            // Save file to server
            string folder = Server.MapPath("~/UploadedImages/");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid() + ext;
            string path = Path.Combine(folder, fileName);
            file.SaveAs(path);

            // Save record to DB
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"INSERT INTO Images (FileName, FilePath, Uploader)
                                 VALUES (@FileName, @FilePath, @Uploader)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FileName", file.FileName);
                    cmd.Parameters.AddWithValue("@FilePath", "/UploadedImages/" + fileName);
                    cmd.Parameters.AddWithValue("@Uploader", Session["Email"].ToString());
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Success"] = "Тасвир бомуваффақият бор карда шуд!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public FileResult Reports()
        {


            return File(Server.MapPath("~/Reports/report.pdf"), "application/pdf", "report.pdf");
        }
}
}