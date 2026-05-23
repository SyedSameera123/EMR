using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Models
{
    public class Ward
    {
        public int WardId { get; set; }
        public string WardName { get; set; }
        public string WardType { get; set; }
        public int Floor { get; set; }
        public int TotalBeds { get; set; }
        public int AvailableBeds { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public int OccupiedBeds => TotalBeds - AvailableBeds;
        public double OccupancyRate => TotalBeds > 0 ? (double)OccupiedBeds / TotalBeds * 100 : 0;
    }
}
