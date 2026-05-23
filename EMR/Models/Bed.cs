using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Models
{
    public class Bed
    {
        public int BedId { get; set; }
        public string BedNumber { get; set; }
        public int WardId { get; set; }
        public string BedType { get; set; }
        public string Status { get; set; }
        public decimal DailyCharge { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public string WardName { get; set; }

        public int Floor { get; set; }


    }
}
