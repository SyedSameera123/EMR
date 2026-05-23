using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMR.Data;
using EMR.Models;

namespace EMR.Services
{
    public class CaretakerService
    {
        public static List<Caretaker> GetAllCaretakers()
        {
            var caretakers = new List<Caretaker>();

            try
            {
                string query = @"
                    SELECT CaretakerId, FirstName, LastName, Specialization, LicenseNumber,
                           PhoneNumber, Email, Address, JoinDate, Shift, IsActive, CreatedDate
                    FROM Caretakers
                    WHERE IsActive = 1
                    ORDER BY LastName, FirstName";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    caretakers.Add(MapRowToCaretaker(row));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve caretakers: {ex.Message}", ex);
            }

            return caretakers;
        }

        public static Caretaker GetCaretakerById(int caretakerId)
        {
            try
            {
                string query = @"
                    SELECT CaretakerId, FirstName, LastName, Specialization, LicenseNumber,
                           PhoneNumber, Email, Address, JoinDate, Shift, IsActive, CreatedDate
                    FROM Caretakers
                    WHERE CaretakerId = @CaretakerId";

                var parameters = new SqlParameter[] { new SqlParameter("@CaretakerId", caretakerId) };
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    return MapRowToCaretaker(dt.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve caretaker: {ex.Message}", ex);
            }
        }

        public static bool AddCaretaker(Caretaker caretaker)
        {
            try
            {
                string query = @"
                    INSERT INTO Caretakers (FirstName, LastName, Specialization, LicenseNumber,
                                          PhoneNumber, Email, Address, JoinDate, Shift)
                    VALUES (@FirstName, @LastName, @Specialization, @LicenseNumber,
                           @PhoneNumber, @Email, @Address, @JoinDate, @Shift)";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@FirstName", caretaker.FirstName),
                    new SqlParameter("@LastName", caretaker.LastName),
                    new SqlParameter("@Specialization", caretaker.Specialization ?? (object)DBNull.Value),
                    new SqlParameter("@LicenseNumber", caretaker.LicenseNumber ?? (object)DBNull.Value),
                    new SqlParameter("@PhoneNumber", caretaker.PhoneNumber),
                    new SqlParameter("@Email", caretaker.Email ?? (object)DBNull.Value),
                    new SqlParameter("@Address", caretaker.Address ?? (object)DBNull.Value),
                    new SqlParameter("@JoinDate", caretaker.JoinDate),
                    new SqlParameter("@Shift", caretaker.Shift ?? (object)DBNull.Value)
                };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add caretaker: {ex.Message}", ex);
            }
        }

        public static bool UpdateCaretaker(Caretaker caretaker)
        {
            try
            {
                string query = @"
                    UPDATE Caretakers
                    SET FirstName = @FirstName, LastName = @LastName, 
                        Specialization = @Specialization, LicenseNumber = @LicenseNumber,
                        PhoneNumber = @PhoneNumber, Email = @Email, Address = @Address,
                        JoinDate = @JoinDate, Shift = @Shift
                    WHERE CaretakerId = @CaretakerId";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@CaretakerId", caretaker.CaretakerId),
                    new SqlParameter("@FirstName", caretaker.FirstName),
                    new SqlParameter("@LastName", caretaker.LastName),
                    new SqlParameter("@Specialization", caretaker.Specialization ?? (object)DBNull.Value),
                    new SqlParameter("@LicenseNumber", caretaker.LicenseNumber ?? (object)DBNull.Value),
                    new SqlParameter("@PhoneNumber", caretaker.PhoneNumber),
                    new SqlParameter("@Email", caretaker.Email ?? (object)DBNull.Value),
                    new SqlParameter("@Address", caretaker.Address ?? (object)DBNull.Value),
                    new SqlParameter("@JoinDate", caretaker.JoinDate),
                    new SqlParameter("@Shift", caretaker.Shift ?? (object)DBNull.Value)
                };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update caretaker: {ex.Message}", ex);
            }
        }

        public static bool DeleteCaretaker(int caretakerId)
        {
            try
            {
                string query = "UPDATE Caretakers SET IsActive = 0 WHERE CaretakerId = @CaretakerId";
                var parameters = new SqlParameter[] { new SqlParameter("@CaretakerId", caretakerId) };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete caretaker: {ex.Message}", ex);
            }
        }

        public static List<PatientCaretakerAssignment> GetAssignments()
        {
            var assignments = new List<PatientCaretakerAssignment>();

            try
            {
                string query = @"
                    SELECT a.AssignmentId, a.PatientId, a.CaretakerId, a.AssignedDate, 
                           a.EndDate, a.Notes, a.IsActive,
                           p.FirstName + ' ' + p.LastName AS PatientName,
                           c.FirstName + ' ' + c.LastName AS CaretakerName
                    FROM PatientCaretakerAssignments a
                    INNER JOIN Patients p ON a.PatientId = p.PatientId
                    INNER JOIN Caretakers c ON a.CaretakerId = c.CaretakerId
                    WHERE a.IsActive = 1
                    ORDER BY a.AssignedDate DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    assignments.Add(new PatientCaretakerAssignment
                    {
                        AssignmentId = Convert.ToInt32(row["AssignmentId"]),
                        PatientId = Convert.ToInt32(row["PatientId"]),
                        CaretakerId = Convert.ToInt32(row["CaretakerId"]),
                        AssignedDate = Convert.ToDateTime(row["AssignedDate"]),
                        EndDate = row["EndDate"] != DBNull.Value ? Convert.ToDateTime(row["EndDate"]) : (DateTime?)null,
                        Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : null,
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        PatientName = row["PatientName"].ToString(),
                        CaretakerName = row["CaretakerName"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve assignments: {ex.Message}", ex);
            }

            return assignments;
        }

        public static bool AddAssignment(PatientCaretakerAssignment assignment)
        {
            try
            {
                string query = @"
                    INSERT INTO PatientCaretakerAssignments (PatientId, CaretakerId, AssignedDate, EndDate,Notes)
                    VALUES (@PatientId, @CaretakerId, @AssignedDate, @EndDate,@Notes)";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@PatientId", assignment.PatientId),
                    new SqlParameter("@CaretakerId", assignment.CaretakerId),
                    new SqlParameter("@AssignedDate", assignment.AssignedDate),
                    new SqlParameter("@EndDate", assignment.EndDate ?? (object)DBNull.Value),
                    new SqlParameter("@Notes", assignment.Notes ?? (object)DBNull.Value)
                };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add assignment: {ex.Message}", ex);
            }
        }

        public static bool EndAssignment(int assignmentId, DateTime endDate)
        {
            try
            {
                string query = @"
                    UPDATE PatientCaretakerAssignments
                    SET EndDate = @EndDate, IsActive = 0
                    WHERE AssignmentId = @AssignmentId";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@AssignmentId", assignmentId),
                    new SqlParameter("@EndDate", endDate)
                };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to end assignment: {ex.Message}", ex);
            }
        }

        public static void AssignPatient(PatientCaretakerAssignment assignment)
        {
            string query = @"INSERT INTO PatientCaretakerAssignments 
                     (PatientId, CaretakerId, AssignedDate, EndDate, Notes, IsActive) 
                     VALUES 
                     (@PatientId, @CaretakerId, @AssignedDate, @EndDate, @Notes, @IsActive)";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@PatientId", assignment.PatientId);
                command.Parameters.AddWithValue("@CaretakerId", assignment.CaretakerId);

                // Use SqlParameter with explicit type for dates
                command.Parameters.Add("@AssignedDate", SqlDbType.DateTime).Value = assignment.AssignedDate;

                // For EndDate - handle nullable DateTime properly
                if (assignment.EndDate.HasValue)
                {
                    command.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = assignment.EndDate.Value;
                }
                else
                {
                    command.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = DBNull.Value;
                }

                command.Parameters.AddWithValue("@Notes", assignment.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", true);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static Caretaker MapRowToCaretaker(DataRow row)
        {
            return new Caretaker
            {
                CaretakerId = Convert.ToInt32(row["CaretakerId"]),
                FirstName = row["FirstName"].ToString(),
                LastName = row["LastName"].ToString(),
                Specialization = row["Specialization"] != DBNull.Value ? row["Specialization"].ToString() : null,
                LicenseNumber = row["LicenseNumber"] != DBNull.Value ? row["LicenseNumber"].ToString() : null,
                PhoneNumber = row["PhoneNumber"].ToString(),
                Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : null,
                Address = row["Address"] != DBNull.Value ? row["Address"].ToString() : null,
                JoinDate = Convert.ToDateTime(row["JoinDate"]),
                Shift = row["Shift"] != DBNull.Value ? row["Shift"].ToString() : null,
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }
    }
}
