using Auth.DAL;
using Auth.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Auth.Controllers
{
    public class PostsController : Controller
    {

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return Content("You must login first.");
            }

            List<Post> posts = new List<Post>();

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string query = "SELECT * FROM Posts ORDER BY CreatedAt DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        posts.Add(new Post
                        {
                            Id = (int)reader["Id"],
                            Title = reader["Title"].ToString(),
                            Content = reader["Content"].ToString(),
                            Author = reader["Author"].ToString(),
                            CreatedAt = (DateTime)reader["CreatedAt"]
                        });
                    }
                }
            }

            return View(posts);
        }


        public ActionResult Create()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        [HttpPost]
        public ActionResult Create(Post post)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string query = @"INSERT INTO Posts (Title,Content,Author)
                                 VALUES (@Title,@Content,@Author)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", post.Title);
                    cmd.Parameters.AddWithValue("@Content", post.Content);

                    cmd.Parameters.AddWithValue("@Author", Session["Username"].ToString());

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }


        public ActionResult Edit(int id)
        {
            if (Session["Username"] == null)
            {
                return Content("You are not authorized. Please login.");
            }

            Post post = null;

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string query = "SELECT * FROM Posts WHERE Id=@Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        post = new Post()
                        {
                            Id = (int)reader["Id"],
                            Title = reader["Title"].ToString(),
                            Content = reader["Content"].ToString(),
                            Author = reader["Author"].ToString()
                        };
                    }
                }
            }

            // SECURITY CHECK
            if (post == null)
            {
                return Content("Post not found.");
            }

            string role = Session["Role"].ToString();

            if (post.Author != Session["Username"].ToString()
                && role != "Admin"
                && role != "Editor"
                && role != "Moderator")
            {
                return Content("You cannot edit this post.");
            }

            return View(post);
        }


        [HttpPost]
        public ActionResult Edit(Post post)
        {
            if (Session["Username"] == null)
            {
                return Content("You are not authorized.");
            }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string query = @"UPDATE Posts
                         SET Title=@Title, Content=@Content
                         WHERE Id=@Id";

                string role = Session["Role"].ToString();

                if (role == "User")
                {
                    query += " AND Author=@Author";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", post.Title);
                    cmd.Parameters.AddWithValue("@Content", post.Content);
                    cmd.Parameters.AddWithValue("@Id", post.Id);

                    if (role == "User")
                    {
                        cmd.Parameters.AddWithValue("@Author", Session["Username"].ToString());
                    }

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }




        //DELETING POST
        public ActionResult Delete(int id)
        {
            if (Session["Role"] == null)
                return RedirectToAction("Login", "Account");

            string role = Session["Role"].ToString();

            if (role != "Moderator" && role != "Admin")
                return Content("Access denied.");

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string query = "DELETE FROM Posts WHERE Id=@Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }


    }
}