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
    public class BedService
    {
        public static List<Ward> GetAllWards()
        {
            var wards = new List<Ward>();
            try
            {
                string query = @"SELECT WardId, WardName, WardType, Floor, TotalBeds, AvailableBeds, IsActive, CreatedDate
                               FROM Wards WHERE IsActive = 1 ORDER BY Floor, WardName";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                foreach (DataRow row in dt.Rows)
                {
                    wards.Add(new Ward
                    {
                        WardId = Convert.ToInt32(row["WardId"]),
                        WardName = row["WardName"].ToString(),
                        WardType = row["WardType"].ToString(),
                        Floor = Convert.ToInt32(row["Floor"]),
                        TotalBeds = Convert.ToInt32(row["TotalBeds"]),
                        AvailableBeds = Convert.ToInt32(row["AvailableBeds"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve wards: {ex.Message}", ex);
            }
            return wards;
        }

        public static List<Bed> GetBedsByWard(int wardId)
        {
            var beds = new List<Bed>();
            try
            {
                string query = @"SELECT b.BedId, b.BedNumber, b.WardId, b.BedType, b.Status, b.DailyCharge, b.IsActive, b.CreatedDate, w.WardName
                               FROM Beds b INNER JOIN Wards w ON b.WardId = w.WardId
                               WHERE b.WardId = @WardId AND b.IsActive = 1 ORDER BY b.BedNumber";
                var parameters = new SqlParameter[] { new SqlParameter("@WardId", wardId) };
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
                foreach (DataRow row in dt.Rows)
                {
                    beds.Add(new Bed
                    {
                        BedId = Convert.ToInt32(row["BedId"]),
                        BedNumber = row["BedNumber"].ToString(),
                        WardId = Convert.ToInt32(row["WardId"]),
                        BedType = row["BedType"].ToString(),
                        Status = row["Status"].ToString(),
                        DailyCharge = Convert.ToDecimal(row["DailyCharge"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        WardName = row["WardName"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve beds: {ex.Message}", ex);
            }
            return beds;
        }

        public static List<Bed> GetAllBeds()
        {
            var beds = new List<Bed>();
            try
            {
                string query = @"SELECT b.BedId, b.BedNumber, b.WardId, b.BedType, b.Status, b.DailyCharge, b.IsActive, b.CreatedDate, w.WardName
                               FROM Beds b INNER JOIN Wards w ON b.WardId = w.WardId
                               WHERE b.IsActive = 1 ORDER BY w.WardName, b.BedNumber";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                foreach (DataRow row in dt.Rows)
                {
                    beds.Add(new Bed
                    {
                        BedId = Convert.ToInt32(row["BedId"]),
                        BedNumber = row["BedNumber"].ToString(),
                        WardId = Convert.ToInt32(row["WardId"]),
                        BedType = row["BedType"].ToString(),
                        Status = row["Status"].ToString(),
                        DailyCharge = Convert.ToDecimal(row["DailyCharge"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        WardName = row["WardName"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve beds: {ex.Message}", ex);
            }
            return beds;
        }

        public static List<BedAssignment> GetActiveAssignments()
        {
            var assignments = new List<BedAssignment>();
            try
            {
                string query = @"SELECT ba.*, b.BedNumber, w.WardName,
                               p.FirstName + ' ' + p.LastName AS PatientName,
                               'Dr. ' + d.FirstName + ' ' + d.LastName AS DoctorName
                               FROM BedAssignments ba
                               INNER JOIN Beds b ON ba.BedId = b.BedId
                               INNER JOIN Wards w ON b.WardId = w.WardId
                               INNER JOIN Patients p ON ba.PatientId = p.PatientId
                               INNER JOIN Doctors d ON ba.DoctorId = d.DoctorId
                               WHERE ba.Status = 'Active'
                               ORDER BY ba.AdmissionDate DESC";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                foreach (DataRow row in dt.Rows)
                {
                    assignments.Add(new BedAssignment
                    {
                        AssignmentId = Convert.ToInt32(row["AssignmentId"]),
                        BedId = Convert.ToInt32(row["BedId"]),
                        PatientId = Convert.ToInt32(row["PatientId"]),
                        DoctorId = Convert.ToInt32(row["DoctorId"]),
                        AdmissionDate = Convert.ToDateTime(row["AdmissionDate"]),
                        DischargeDate = row["DischargeDate"] != DBNull.Value ? Convert.ToDateTime(row["DischargeDate"]) : (DateTime?)null,
                        Reason = row["Reason"] != DBNull.Value ? row["Reason"].ToString() : null,
                        Status = row["Status"].ToString(),
                        TotalCharges = Convert.ToDecimal(row["TotalCharges"]),
                        Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : null,
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        BedNumber = row["BedNumber"].ToString(),
                        WardName = row["WardName"].ToString(),
                        PatientName = row["PatientName"].ToString(),
                        DoctorName = row["DoctorName"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve bed assignments: {ex.Message}", ex);
            }
            return assignments;
        }

        public static bool AssignBed(BedAssignment assignment)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert assignment
                        string insertQuery = @"INSERT INTO BedAssignments (BedId, PatientId, DoctorId, AdmissionDate, Reason, Notes)
                                             VALUES (@BedId, @PatientId, @DoctorId, @AdmissionDate, @Reason, @Notes)";
                        using (var cmd = new SqlCommand(insertQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BedId", assignment.BedId);
                            cmd.Parameters.AddWithValue("@PatientId", assignment.PatientId);
                            cmd.Parameters.AddWithValue("@DoctorId", assignment.DoctorId);
                            cmd.Parameters.AddWithValue("@AdmissionDate", assignment.AdmissionDate);
                            cmd.Parameters.AddWithValue("@Reason", assignment.Reason ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Notes", assignment.Notes ?? (object)DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        // Update bed status
                        string updateBedQuery = "UPDATE Beds SET Status = 'Occupied' WHERE BedId = @BedId";
                        using (var cmd = new SqlCommand(updateBedQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BedId", assignment.BedId);
                            cmd.ExecuteNonQuery();
                        }

                        // Update ward available beds
                        string updateWardQuery = @"UPDATE Wards SET AvailableBeds = AvailableBeds - 1 
                                                 WHERE WardId = (SELECT WardId FROM Beds WHERE BedId = @BedId)";
                        using (var cmd = new SqlCommand(updateWardQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BedId", assignment.BedId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }




        public static List<Doctor> GetAllDoctors()
        {
            var doctors = new List<Doctor>();
            try
            {
                string query = @"SELECT DoctorId, FirstName, LastName, Specialization, 
                        LicenseNumber, PhoneNumber, Email, ConsultationFee, IsAvailable, CreatedDate
                        FROM Doctors 
                        WHERE IsAvailable = 1 
                        ORDER BY FirstName, LastName";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    doctors.Add(new Doctor
                    {
                        DoctorId = Convert.ToInt32(row["DoctorId"]),
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString(),
                        Specialization = row["Specialization"].ToString(),
                        LicenseNumber = row["LicenseNumber"].ToString(),
                        PhoneNumber = row["PhoneNumber"].ToString(),
                        Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : null,
                        ConsultationFee = Convert.ToDecimal(row["ConsultationFee"]),
                        IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve doctors: {ex.Message}", ex);
            }
            return doctors;
        }




        public static bool DischargeBed(int assignmentId, DateTime dischargeDate)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Get bed info
                        string getBedQuery = "SELECT BedId, AdmissionDate FROM BedAssignments WHERE AssignmentId = @AssignmentId";
                        int bedId;
                        DateTime admissionDate;
                        decimal dailyCharge;

                        using (var cmd = new SqlCommand(getBedQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read()) throw new Exception("Assignment not found");
                                bedId = reader.GetInt32(0);
                                admissionDate = reader.GetDateTime(1);
                            }
                        }

                        string getChargeQuery = "SELECT DailyCharge FROM Beds WHERE BedId = @BedId";
                        using (var cmd = new SqlCommand(getChargeQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BedId", bedId);
                            dailyCharge = (decimal)cmd.ExecuteScalar();
                        }

                        // Calculate charges
                        int days = (dischargeDate - admissionDate).Days + 1;
                        decimal totalCharges = days * dailyCharge;

                        // Update assignment
                        string updateAssignmentQuery = @"UPDATE BedAssignments 
                                                       SET DischargeDate = @DischargeDate, Status = 'Discharged', TotalCharges = @TotalCharges
                                                       WHERE AssignmentId = @AssignmentId";
                        using (var cmd = new SqlCommand(updateAssignmentQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
                            cmd.Parameters.AddWithValue("@DischargeDate", dischargeDate);
                            cmd.Parameters.AddWithValue("@TotalCharges", totalCharges);
                            cmd.ExecuteNonQuery();
                        }

                        // Update bed status
                        string updateBedQuery = "UPDATE Beds SET Status = 'Available' WHERE BedId = @BedId";
                        using (var cmd = new SqlCommand(updateBedQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BedId", bedId);
                            cmd.ExecuteNonQuery();
                        }

                        // Update ward available beds
                        string updateWardQuery = @"UPDATE Wards SET AvailableBeds = AvailableBeds + 1 
                                                 WHERE WardId = (SELECT WardId FROM Beds WHERE BedId = @BedId)";
                        using (var cmd = new SqlCommand(updateWardQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BedId", bedId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
