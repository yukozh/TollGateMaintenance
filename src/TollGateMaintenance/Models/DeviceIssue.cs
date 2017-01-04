using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TollGateMaintenance.Models
{
    public class DeviceIssue
    {
        public string Name { get; set; }

        public string Lane { get; set; }

        public string Phenomenon { get; set; }

        public int Count { get; set; } = 1;

        public string Solution { get; set; }

        public bool IsSolved { get; set; }

        public string Raw { get; set; }
    }
}
