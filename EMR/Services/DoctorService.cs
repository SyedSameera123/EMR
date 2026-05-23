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
    public class DoctorService
    {
        public static List<Doctor> GetAllDoctors()
        {
            var doctors = new List<Doctor>();
            try
            {
                string query = @"SELECT DoctorId, FirstName, LastName, Specialization, LicenseNumber,
                               PhoneNumber, Email, ConsultationFee, IsAvailable, CreatedDate
                               FROM Doctors ORDER BY LastName, FirstName";
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

        public static bool AddDoctor(Doctor doctor)
        {
            try
            {
                string query = @"INSERT INTO Doctors (FirstName, LastName, Specialization, LicenseNumber,
                               PhoneNumber, Email, ConsultationFee)
                               VALUES (@FirstName, @LastName, @Specialization, @LicenseNumber,
                               @PhoneNumber, @Email, @ConsultationFee)";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@FirstName", doctor.FirstName),
                    new SqlParameter("@LastName", doctor.LastName),
                    new SqlParameter("@Specialization", doctor.Specialization),
                    new SqlParameter("@LicenseNumber", doctor.LicenseNumber),
                    new SqlParameter("@PhoneNumber", doctor.PhoneNumber),
                    new SqlParameter("@Email", doctor.Email ?? (object)DBNull.Value),
                    new SqlParameter("@ConsultationFee", doctor.ConsultationFee)
                };
                return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add doctor: {ex.Message}", ex);
            }
        }

        public static bool UpdateDoctor(Doctor doctor)
        {
            try
            {
                string query = @"UPDATE Doctors SET FirstName = @FirstName, LastName = @LastName,
                               Specialization = @Specialization, LicenseNumber = @LicenseNumber,
                               PhoneNumber = @PhoneNumber, Email = @Email, ConsultationFee = @ConsultationFee,
                               IsAvailable = @IsAvailable WHERE DoctorId = @DoctorId";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@DoctorId", doctor.DoctorId),
                    new SqlParameter("@FirstName", doctor.FirstName),
                    new SqlParameter("@LastName", doctor.LastName),
                    new SqlParameter("@Specialization", doctor.Specialization),
                    new SqlParameter("@LicenseNumber", doctor.LicenseNumber),
                    new SqlParameter("@PhoneNumber", doctor.PhoneNumber),
                    new SqlParameter("@Email", doctor.Email ?? (object)DBNull.Value),
                    new SqlParameter("@ConsultationFee", doctor.ConsultationFee),
                    new SqlParameter("@IsAvailable", doctor.IsAvailable)
                };
                return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update doctor: {ex.Message}", ex);
            }
        }
    }
}
