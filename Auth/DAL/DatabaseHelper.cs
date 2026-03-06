using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Auth.DAL
{
    public class DatabaseHelper
    {
        public static SqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AuthDBConnection"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }
    }
}