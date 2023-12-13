using System;
using System.Collections.Generic;
namespace MVC_Read_Excel.Models;

public partial class AttAttendanceLog
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? Username { get; set; }

    public DateTime AttDate { get; set; }

    public DateTime LogTime { get; set; }

    public string? Createdby { get; set; }

    public DateTime? Createddate { get; set; }
}
