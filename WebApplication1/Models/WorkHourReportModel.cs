using System;
using System.Collections.Generic;
namespace MVC_Read_Excel.Models
{
    public class WorkHourReportModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<WorkHourReportEntry> ReportEntries { get; set; }
        public TimeSpan LoginTime { get; set; } 
        public TimeSpan LogoutTime { get; set; }
        public TimeSpan GroupTotal { get; set; }
    }

    public class WorkHourReportEntry
    {
        public DateTime AttDate { get; set; }
        public TimeSpan WorkHour { get; set; }
        public TimeSpan LoginTime { get; set; } 
        public TimeSpan LogoutTime { get; set; }
    }

}
