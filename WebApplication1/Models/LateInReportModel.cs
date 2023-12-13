using System;
using System.Collections.Generic;
namespace MVC_Read_Excel.Models
{
    public class LateInReportModel
    {
        public List<LateInReportModel> ReportData { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public TimeSpan LoginTime { get; set; } 
        public TimeSpan LogoutTime { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<LateInReportEntry> ReportEntries { get; set; }
        public TimeSpan GroupTotal { get; set; }

    }

    public class LateInReportEntry
    {
        public DateTime AttDate { get; set; }
        public TimeSpan LoginTime { get; set; } 
        public TimeSpan LogoutTime { get; set; }
        public TimeSpan LateInMinute { get; set; }
    }

}

