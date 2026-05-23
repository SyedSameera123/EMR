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
    public static class PatientService
    {
        public static List<Patient> GetAllPatients()
        {
            var patients = new List<Patient>();
            string query = "SELECT * FROM Patients WHERE IsActive = 1 ORDER BY FirstName, LastName";

            try
            {
                var table = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in table.Rows)
                {
                    DateTime dob = Convert.ToDateTime(row["DateOfBirth"]);

                    // Always calculate age from DOB
                    int calculatedAge = CalculateAge(dob);

                    patients.Add(new Patient
                    {
                        PatientId = Convert.ToInt32(row["PatientId"]),
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString(),
                        DateOfBirth = dob,
                        Age = calculatedAge,  // Use calculated age
                        Gender = row["Gender"].ToString(),
                        BloodGroup = row["BloodGroup"].ToString(),
                        PhoneNumber = row["PhoneNumber"].ToString(),
                        Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : null,
                        Address = row["Address"] != DBNull.Value ? row["Address"].ToString() : null,
                        City = row["City"] != DBNull.Value ? row["City"].ToString() : null,
                        State = row.Table.Columns.Contains("State") && row["State"] != DBNull.Value
                            ? row["State"].ToString()
                            : null,
                        EmergencyContactName = row["EmergencyContactName"] != DBNull.Value
                            ? row["EmergencyContactName"].ToString()
                            : null,
                        EmergencyContactPhone = row["EmergencyContactPhone"] != DBNull.Value
                            ? row["EmergencyContactPhone"].ToString()
                            : null,
                        RegistrationDate = row.Table.Columns.Contains("RegistrationDate") && row["RegistrationDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["RegistrationDate"])
                            : DateTime.Now,
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllPatients error: {ex.Message}");
                throw;
            }

            return patients;
        }

        public static void AddPatient(Patient patient)
        {
            // Calculate age if not already set
            if (patient.Age == 0)
            {
                patient.Age = CalculateAge(patient.DateOfBirth);
            }

            // Try INSERT with Age and RegistrationDate first
            try
            {
                string query = @"INSERT INTO Patients 
                    (FirstName, LastName, DateOfBirth, Age, Gender, BloodGroup, 
                     PhoneNumber, Email, Address, City, State, 
                     EmergencyContactName, EmergencyContactPhone, 
                     RegistrationDate, IsActive)
                    VALUES 
                    (@FirstName, @LastName, @DateOfBirth, @Age, @Gender, @BloodGroup,
                     @PhoneNumber, @Email, @Address, @City, @State,
                     @EmergencyContactName, @EmergencyContactPhone,
                     @RegistrationDate, @IsActive)";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@FirstName", patient.FirstName),
                    new SqlParameter("@LastName", patient.LastName),
                    new SqlParameter("@DateOfBirth", patient.DateOfBirth),
                    new SqlParameter("@Age", patient.Age),
                    new SqlParameter("@Gender", patient.Gender),
                    new SqlParameter("@BloodGroup", patient.BloodGroup),
                    new SqlParameter("@PhoneNumber", patient.PhoneNumber),
                    new SqlParameter("@Email", (object)patient.Email ?? DBNull.Value),
                    new SqlParameter("@Address", (object)patient.Address ?? DBNull.Value),
                    new SqlParameter("@City", (object)patient.City ?? DBNull.Value),
                    new SqlParameter("@State", (object)patient.State ?? DBNull.Value),
                    new SqlParameter("@EmergencyContactName", (object)patient.EmergencyContactName ?? DBNull.Value),
                    new SqlParameter("@EmergencyContactPhone", (object)patient.EmergencyContactPhone ?? DBNull.Value),
                    new SqlParameter("@RegistrationDate", patient.RegistrationDate),
                    new SqlParameter("@IsActive", patient.IsActive)
                };

                DatabaseHelper.ExecuteNonQuery(query, parameters);
            }
            catch (SqlException ex) when (ex.Message.Contains("Invalid column name"))
            {
                // If Age or RegistrationDate columns don't exist, try without them
                System.Diagnostics.Debug.WriteLine("Columns don't exist, trying alternative INSERT");

                string queryAlt = @"INSERT INTO Patients 
                    (FirstName, LastName, DateOfBirth, Gender, BloodGroup, 
                     PhoneNumber, Email, Address, City,
                     EmergencyContactName, EmergencyContactPhone, IsActive)
                    VALUES 
                    (@FirstName, @LastName, @DateOfBirth, @Gender, @BloodGroup,
                     @PhoneNumber, @Email, @Address, @City,
                     @EmergencyContactName, @EmergencyContactPhone, @IsActive)";

                var parametersAlt = new SqlParameter[]
                {
                    new SqlParameter("@FirstName", patient.FirstName),
                    new SqlParameter("@LastName", patient.LastName),
                    new SqlParameter("@DateOfBirth", patient.DateOfBirth),
                    new SqlParameter("@Gender", patient.Gender),
                    new SqlParameter("@BloodGroup", patient.BloodGroup),
                    new SqlParameter("@PhoneNumber", patient.PhoneNumber),
                    new SqlParameter("@Email", (object)patient.Email ?? DBNull.Value),
                    new SqlParameter("@Address", (object)patient.Address ?? DBNull.Value),
                    new SqlParameter("@City", (object)patient.City ?? DBNull.Value),
                    new SqlParameter("@EmergencyContactName", (object)patient.EmergencyContactName ?? DBNull.Value),
                    new SqlParameter("@EmergencyContactPhone", (object)patient.EmergencyContactPhone ?? DBNull.Value),
                    new SqlParameter("@IsActive", patient.IsActive)
                };

                DatabaseHelper.ExecuteNonQuery(queryAlt, parametersAlt);
            }
        }

        public static void UpdatePatient(Patient patient)
        {
            patient.Age = CalculateAge(patient.DateOfBirth);

            string query = @"UPDATE Patients SET 
                FirstName = @FirstName,
                LastName = @LastName,
                DateOfBirth = @DateOfBirth,
                Gender = @Gender,
                BloodGroup = @BloodGroup,
                PhoneNumber = @PhoneNumber,
                Email = @Email,
                Address = @Address,
                City = @City,
                EmergencyContactName = @EmergencyContactName,
                EmergencyContactPhone = @EmergencyContactPhone
                WHERE PatientId = @PatientId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PatientId", patient.PatientId),
                new SqlParameter("@FirstName", patient.FirstName),
                new SqlParameter("@LastName", patient.LastName),
                new SqlParameter("@DateOfBirth", patient.DateOfBirth),
                new SqlParameter("@Gender", patient.Gender),
                new SqlParameter("@BloodGroup", patient.BloodGroup),
                new SqlParameter("@PhoneNumber", patient.PhoneNumber),
                new SqlParameter("@Email", (object)patient.Email ?? DBNull.Value),
                new SqlParameter("@Address", (object)patient.Address ?? DBNull.Value),
                new SqlParameter("@City", (object)patient.City ?? DBNull.Value),
                new SqlParameter("@EmergencyContactName", (object)patient.EmergencyContactName ?? DBNull.Value),
                new SqlParameter("@EmergencyContactPhone", (object)patient.EmergencyContactPhone ?? DBNull.Value)
            };

            DatabaseHelper.ExecuteNonQuery(query, parameters);
        }

        public static void DeletePatient(int patientId)
        {
            string query = "UPDATE Patients SET IsActive = 0 WHERE PatientId = @PatientId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PatientId", patientId)
            };

            DatabaseHelper.ExecuteNonQuery(query, parameters);
        }

        private static int CalculateAge(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
