using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Models
{
    public class BedAssignment
    {
        //public int AssignmentId { get; set; }
        //public int BedId { get; set; }
        //public int PatientId { get; set; }
        //public int DoctorId { get; set; }
        //public DateTime AdmissionDate { get; set; }
        //public DateTime? DischargeDate { get; set; }
        //public string Reason { get; set; }
        //public string Status { get; set; }
        //public decimal TotalCharges { get; set; }
        //public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public string BedNumber { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string WardName { get; set; }

        //public int DaysAdmitted => DischargeDate.HasValue
        //    ? (DischargeDate.Value - AdmissionDate).Days + 1
        //    : (DateTime.Now - AdmissionDate).Days + 1;


        public string AdmissionReason { get; set; }  // ← Add this!
        public DateTime? ExpectedDischargeDate { get; set; }  // ← Add this!
    }
}
