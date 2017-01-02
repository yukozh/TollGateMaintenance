using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TollGateMaintenance.Models
{
    public class Report
    {
        public Guid Id { get; set; }

        [MaxLength(256)]
        public string Management { get; set; } = "";

        [MaxLength(256)]
        public string TollGate { get; set; } = "";

        public DateTime Time { get; set; }

        public JsonObject<IEnumerable<DeviceIssue>> Issues { get; set; } = "[]";
        
        public string FileName { get; set; }

        public byte[] FileBlob { get; set; }

        public string RawHtml { get; set; }

        public string RawText { get; set; }
    }
}
