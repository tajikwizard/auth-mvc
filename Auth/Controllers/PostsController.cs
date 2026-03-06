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

            // Only author or admin can edit
            if (post.Author != Session["Username"].ToString() && Session["Role"].ToString() != "Admin")
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

                // If not admin, restrict update to own posts
                if (Session["Role"].ToString() != "Admin")
                {
                    query += " AND Author=@Author";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", post.Title);
                    cmd.Parameters.AddWithValue("@Content", post.Content);
                    cmd.Parameters.AddWithValue("@Id", post.Id);

                    if (Session["Role"].ToString() != "Admin")
                    {
                        cmd.Parameters.AddWithValue("@Author", Session["Username"].ToString());
                    }

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }


    }
}