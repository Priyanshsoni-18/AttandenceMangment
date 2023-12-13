using System;
using System.Collections.Generic;

namespace MVC_Read_Excel.Models;
public partial class AttAttendanceRegister
{
 
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public DateTime AttDate { get; set; }

    public decimal LateInMinute { get; set; }

    public decimal EarlyOutMinute { get; set; }

    public decimal WorkHour { get; set; }

    public decimal? OverTimeMinute { get; set; }

    public bool? IsLeave { get; set; }

    public bool? IsHoliday { get; set; }

    public bool? IsWeekOff { get; set; }

    public DateTime? ProcessDate { get; set; }

    public string? ProcessBy { get; set; }


    public static implicit operator AttAttendanceRegister(AttAttendanceLog v)
    {
        throw new NotImplementedException();
    }
}
