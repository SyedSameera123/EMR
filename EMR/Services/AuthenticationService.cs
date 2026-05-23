using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EMR.Data;
using EMR.Models;

namespace EMR.Services
{
    public class AuthenticationService
    {
        private static User _currentUser;

        public static User CurrentUser
        {
            get => _currentUser;
            private set => _currentUser = value;
        }

        public static User Login(string username, string password)
        {
            try
            {
                string passwordHash = HashPassword(password);

                string query = @"
            SELECT u.UserId, u.Username, u.Email, u.FirstName, u.LastName, 
                   u.RoleId, r.RoleName, u.PhoneNumber, u.IsActive, u.CreatedDate
            FROM Users u
            INNER JOIN Roles r ON u.RoleId = r.RoleId
            WHERE u.Username = @Username AND u.PasswordHash = @PasswordHash AND u.IsActive = 1";

                var parameters = new SqlParameter[]
                {
            new SqlParameter("@Username", username),
            new SqlParameter("@PasswordHash", passwordHash)
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    CurrentUser = new User
                    {
                        UserId = Convert.ToInt32(row["UserId"]),
                        Username = row["Username"].ToString(),
                        Email = row["Email"].ToString(),
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString(),
                        RoleId = Convert.ToInt32(row["RoleId"]),
                        RoleName = row["RoleName"].ToString(),
                        PhoneNumber = row["PhoneNumber"].ToString(),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                    };

                    // Update last login
                    UpdateLastLogin(CurrentUser.UserId);

                    return CurrentUser;  // Return the user object
                }

                return null;  // Return null if login fails
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}", ex);
            }
        }

        public static bool Register(string username, string password, string email, string firstName,
                                   string lastName, int roleId, string phoneNumber)
        {
            try
            {
                string passwordHash = HashPassword(password);

                string query = @"
                    INSERT INTO Users (Username, PasswordHash, Email, FirstName, LastName, RoleId, PhoneNumber)
                    VALUES (@Username, @PasswordHash, @Email, @FirstName, @LastName, @RoleId, @PhoneNumber)";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@PasswordHash", passwordHash),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@FirstName", firstName),
                    new SqlParameter("@LastName", lastName),
                    new SqlParameter("@RoleId", roleId),
                    new SqlParameter("@PhoneNumber", phoneNumber ?? (object)DBNull.Value)
                };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Registration failed: {ex.Message}", ex);
            }
        }

        public static bool IsUsernameAvailable(string username)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            var parameters = new SqlParameter[] { new SqlParameter("@Username", username) };

            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
            return count == 0;
        }

        public static bool IsEmailAvailable(string email)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            var parameters = new SqlParameter[] { new SqlParameter("@Email", email) };

            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
            return count == 0;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        private static void UpdateLastLogin(int userId)
        {
            string query = "UPDATE Users SET LastLogin = GETDATE() WHERE UserId = @UserId";
            var parameters = new SqlParameter[] { new SqlParameter("@UserId", userId) };
            DatabaseHelper.ExecuteNonQuery(query, parameters);
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("X2"));
                }
                return builder.ToString();
            }
        }

        public static bool HasPermission(string requiredRole)
        {
            if (CurrentUser == null) return false;

            // Administrator has all permissions
            if (CurrentUser.RoleName == "Administrator") return true;

            return CurrentUser.RoleName == requiredRole;
        }

        public static bool HasAnyPermission(params string[] roles)
        {
            if (CurrentUser == null) return false;

            // Administrator has all permissions
            if (CurrentUser.RoleName == "Administrator") return true;

            foreach (string role in roles)
            {
                if (CurrentUser.RoleName == role) return true;
            }

            return false;
        }
    }
}
