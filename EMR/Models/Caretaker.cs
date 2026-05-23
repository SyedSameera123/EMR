using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Models
{
    public class Caretaker
    {
        public int CaretakerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
        public string LicenseNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime JoinDate { get; set; }
        public string Shift { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}