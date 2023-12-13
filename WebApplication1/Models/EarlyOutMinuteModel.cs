namespace MVC_Read_Excel.Models
{
    public class EarlyOutMinuteModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<EarlyOutMinuteEntry> ReportEntries { get; set; }
        public TimeSpan LoginTime { get; set; } 
        public TimeSpan LogoutTime { get; set; }
        public TimeSpan GroupTotal { get; set; }
    }

    public class EarlyOutMinuteEntry
    {
        public DateTime AttDate { get; set; }
        public TimeSpan LoginTime { get; set; } 
        public TimeSpan LogoutTime { get; set; }
        public TimeSpan EarlyOutMinute { get; set; }
    }

}