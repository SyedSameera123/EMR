using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMR.Data;
using EMR.Models;

namespace EMR.Services
{
    public class RoleService
    {
        public static List<Role> GetAllRoles()
        {
            var roles = new List<Role>();

            try
            {
                string query = "SELECT RoleId, RoleName, Description, CreatedDate, IsActive FROM Roles WHERE IsActive = 1";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    roles.Add(new Role
                    {
                        RoleId = Convert.ToInt32(row["RoleId"]),
                        RoleName = row["RoleName"].ToString(),
                        Description = row["Description"].ToString(),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve roles: {ex.Message}", ex);
            }

            return roles;
        }
    }
}
