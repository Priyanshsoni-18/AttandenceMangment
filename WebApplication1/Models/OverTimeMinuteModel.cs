namespace MVC_Read_Excel.Models
{
    public class OverTimeMinuteModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<OverTimeMinuteEntry> ReportEntries { get; set; }
        public TimeSpan LoginTime { get; set; } 
        public TimeSpan LogoutTime { get; set; }
        public TimeSpan GroupTotal { get; set; }
    }

    public class OverTimeMinuteEntry
    {
        public DateTime AttDate { get; set; }
        public TimeSpan OverTimeMinute { get; set; }
        public TimeSpan LoginTime { get; set; } 
        public TimeSpan LogoutTime { get; set; }
    }

}
