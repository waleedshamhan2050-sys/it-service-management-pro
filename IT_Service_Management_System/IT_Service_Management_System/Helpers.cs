using System;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace IT_Service_Management_System
{
    public static class SecurityHelper
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            string hashOfInput = HashPassword(password);
            return String.Compare(hashOfInput, hash, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }

    public class DatabaseHelper
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\IT_Service_DB.mdf;Integrated Security=True;Connect Timeout=30";

        public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    con.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }

        public void LogAction(int userId, string action)
        {
            string query = "INSERT INTO AuditLogs (UserID, Action, ActionDate) VALUES (@UserID, @Action, @Date)";
            SqlParameter[] parameters = {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@Action", action),
                new SqlParameter("@Date", DateTime.Now)
            };
            ExecuteNonQuery(query, parameters);
        }
    }

    public static class Session
    {
        public static int UserID { get; set; }
        public static string UserName { get; set; }
        public static string Role { get; set; }
    }
}
