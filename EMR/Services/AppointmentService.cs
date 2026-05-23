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
    public  class AppointmentService
    {
        public static List<Appointment> GetAllAppointments()
        {
            var appointments = new List<Appointment>();
            try
            {
                string query = @"SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDate,
                               a.AppointmentType, a.Status, a.Reason, a.Notes, a.CreatedDate, a.UpdatedDate,
                               p.FirstName + ' ' + p.LastName AS PatientName,
                               'Dr. ' + d.FirstName + ' ' + d.LastName AS DoctorName
                               FROM Appointments a
                               INNER JOIN Patients p ON a.PatientId = p.PatientId
                               INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
                               ORDER BY a.AppointmentDate DESC";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                foreach (DataRow row in dt.Rows)
                {
                    appointments.Add(new Appointment
                    {
                        AppointmentId = Convert.ToInt32(row["AppointmentId"]),
                        PatientId = Convert.ToInt32(row["PatientId"]),
                        DoctorId = Convert.ToInt32(row["DoctorId"]),
                        AppointmentDate = Convert.ToDateTime(row["AppointmentDate"]),
                        AppointmentType = row["AppointmentType"].ToString(),
                        Status = row["Status"].ToString(),
                        Reason = row["Reason"] != DBNull.Value ? row["Reason"].ToString() : null,
                        Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : null,
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        UpdatedDate = Convert.ToDateTime(row["UpdatedDate"]),
                        PatientName = row["PatientName"].ToString(),
                        DoctorName = row["DoctorName"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve appointments: {ex.Message}", ex);
            }
            return appointments;
        }

        public static bool AddAppointment(Appointment appointment)
        {
            try
            {
                string query = @"INSERT INTO Appointments (PatientId, DoctorId, AppointmentDate, AppointmentType, Reason, Notes)
                               VALUES (@PatientId, @DoctorId, @AppointmentDate, @AppointmentType, @Reason, @Notes)";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@PatientId", appointment.PatientId),
                    new SqlParameter("@DoctorId", appointment.DoctorId),
                    new SqlParameter("@AppointmentDate", appointment.AppointmentDate),
                    new SqlParameter("@AppointmentType", appointment.AppointmentType),
                    new SqlParameter("@Reason", appointment.Reason ?? (object)DBNull.Value),
                    new SqlParameter("@Notes", appointment.Notes ?? (object)DBNull.Value)
                };
                return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add appointment: {ex.Message}", ex);
            }
        }

        public static bool UpdateAppointmentStatus(int appointmentId, string status)
        {
            try
            {
                string query = @"UPDATE Appointments SET Status = @Status, UpdatedDate = GETDATE() WHERE AppointmentId = @AppointmentId";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@AppointmentId", appointmentId),
                    new SqlParameter("@Status", status)
                };
                return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update appointment: {ex.Message}", ex);
            }
        }

        public static List<Prescription> GetPrescriptionsByAppointment(int appointmentId)
        {
            var prescriptions = new List<Prescription>();
            try
            {
                string query = @"SELECT p.*, pt.FirstName + ' ' + pt.LastName AS PatientName,
                               'Dr. ' + d.FirstName + ' ' + d.LastName AS DoctorName
                               FROM Prescriptions p
                               INNER JOIN Patients pt ON p.PatientId = pt.PatientId
                               INNER JOIN Doctors d ON p.DoctorId = d.DoctorId
                               WHERE p.AppointmentId = @AppointmentId";
                var parameters = new SqlParameter[] { new SqlParameter("@AppointmentId", appointmentId) };
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
                foreach (DataRow row in dt.Rows)
                {
                    prescriptions.Add(new Prescription
                    {
                        PrescriptionId = Convert.ToInt32(row["PrescriptionId"]),
                        AppointmentId = Convert.ToInt32(row["AppointmentId"]),
                        PatientId = Convert.ToInt32(row["PatientId"]),
                        DoctorId = Convert.ToInt32(row["DoctorId"]),
                        Medication = row["Medication"].ToString(),
                        Dosage = row["Dosage"].ToString(),
                        Frequency = row["Frequency"].ToString(),
                        Duration = row["Duration"].ToString(),
                        Instructions = row["Instructions"] != DBNull.Value ? row["Instructions"].ToString() : null,
                        PrescribedDate = Convert.ToDateTime(row["PrescribedDate"]),
                        PatientName = row["PatientName"].ToString(),
                        DoctorName = row["DoctorName"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve prescriptions: {ex.Message}", ex);
            }
            return prescriptions;
        }
        public static bool AddPrescription(Prescription prescription)
        {
            try
            {
                string query = @"INSERT INTO Prescriptions (AppointmentId, PatientId, DoctorId, Medication, Dosage, Frequency, Duration, Instructions)
                               VALUES (@AppointmentId, @PatientId, @DoctorId, @Medication, @Dosage, @Frequency, @Duration, @Instructions)";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@AppointmentId", prescription.AppointmentId),
                    new SqlParameter("@PatientId", prescription.PatientId),
                    new SqlParameter("@DoctorId", prescription.DoctorId),
                    new SqlParameter("@Medication", prescription.Medication),
                    new SqlParameter("@Dosage", prescription.Dosage),
                    new SqlParameter("@Frequency", prescription.Frequency),
                    new SqlParameter("@Duration", prescription.Duration),
                    new SqlParameter("@Instructions", prescription.Instructions ?? (object)DBNull.Value)
                };
                return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add prescription: {ex.Message}", ex);
            }
        }
    }
}
