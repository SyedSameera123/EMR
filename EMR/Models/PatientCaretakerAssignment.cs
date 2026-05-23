using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Models
{
    public class PatientCaretakerAssignment
    {
        public int AssignmentId { get; set; }
        public int PatientId { get; set; }
        public int CaretakerId { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public string PatientName { get; set; }
        public string CaretakerName { get; set; }
    }
}
