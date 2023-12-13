using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVC_Read_Excel.Models;
using ExcelDataReader;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ExcelDataReader.Log;


namespace MVC_Read_Excel.Controllers
{
    public class DataUploadController : Controller
    {
        
        IConfiguration configuration;
        IWebHostEnvironment hostEnvironment;
        PrContext context;
        IExcelDataReader reader;

        public DataUploadController(IConfiguration configuration, IWebHostEnvironment hostEnvironment, PrContext context)
        {
            this.configuration = configuration;
            this.hostEnvironment = hostEnvironment;
            this.context = context;
        }

        public async Task<IActionResult> View_Data()
        {
            var attAttendanceLogs = await context.AttAttendanceLogs.ToListAsync();
            return View(attAttendanceLogs);
        }

        public async Task<IActionResult> view_all_data()
        {
            var attAttendanceLogs = await context.AttAttendanceLogs.ToListAsync();
            return View(attAttendanceLogs);
        }

        public async Task<IActionResult> Index()
        {
            var attAttendanceLogs = await context.AttAttendanceLogs.ToListAsync();
            return View(attAttendanceLogs);
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file, AttAttendanceRegister? attAttendanceRegisters)
        {
            try
            {
                // Check the File is received
                if (file == null)
                {
                    //   throw new Exception("File is Not Received..." );
                    return View();
                }
                // Create the Directory if it does not exist
                string dirPath = Path.Combine(hostEnvironment.WebRootPath, "ReceivedReports");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // Make sure that only Excel files are used 
                string dataFileName = Path.GetFileName(file.FileName);
                string extension = Path.GetExtension(dataFileName);
                string[] allowedExtensions = new string[] { ".xls", ".xlsx" };

                if (!allowedExtensions.Contains(extension))
                    throw new Exception("Sorry! This file is not allowed. Make sure that the file has an extension of .xls or .xlsx.");


                // Make a copy of the posted file from the received HTTP request
                string saveToPath = Path.Combine(dirPath, dataFileName);

                using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Use this to handle encoding differences in .NET Core
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                // Read the excel file
                using (var stream = new FileStream(saveToPath, FileMode.Open))
                {
                    if (extension == ".xls")
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    else
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                    DataSet ds = new DataSet();
                    ds = reader.AsDataSet();
                    reader.Close();

                    if (ds != null && ds.Tables.Count > 0)
                    {
                        // Read the table
                        DataTable serviceDetails = ds.Tables[0];

                        List<AttAttendanceLog> log = new List<AttAttendanceLog>();

                        for (int i = 1; i < serviceDetails.Rows.Count; i++)
                        {
                            if (!IsRowEmpty(serviceDetails.Rows[i]))
                            {
                                AttAttendanceLog details = new AttAttendanceLog();
                                details.UserId = serviceDetails.Rows[i][0].ToString();
                                details.Username = serviceDetails.Rows[i][1].ToString();
                                details.LogTime = Convert.ToDateTime(serviceDetails.Rows[i][2].ToString());
                                details.AttDate = Convert.ToDateTime(serviceDetails.Rows[i][2].ToString()).Date;
                                details.Createdby = "Priyansh";
                                details.Createddate = DateTime.Now;
                                log.Add(details);
                            }
                        }
                        // Process the attendance log and insert into the database
                        context.AttAttendanceLogs.AddRange(log);
                        await context.SaveChangesAsync();

                    }
                    else
                    {
                        throw new Exception("Failed to read the excel file. Please make sure that the file is not empty and contains valid data.");
                    }
                }

                return RedirectToAction("view_all_data");
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel()
                {
                    ControllerName = this.RouteData.Values["controller"].ToString(),
                    ActionName = this.RouteData.Values["action"].ToString(),
                    ErrorMessage = ex.Message
                });
            }
        }

        public async Task<IActionResult> ProcessData(int month, int year)
        {
            try
            {
                // Get the start and end dates for the selected month and year
                DateTime startDate = new DateTime(year, month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);

                // Retrieve the attendance logs for the selected month
                var attendanceLogs = await context.AttAttendanceLogs
                    .Where(a => a.AttDate >= startDate && a.AttDate <= endDate)
                    .ToListAsync();

                // Process the attendance logs and insert into the attendance registers
                foreach (var attendanceGroup in attendanceLogs.GroupBy(a => new { a.UserId, a.AttDate.Date }))
                {
                    var attendance = attendanceGroup.First();
                    // Take the first record as reference

                    var login = Convert.ToDateTime(attendanceGroup.Min(a => a.LogTime)).TimeOfDay;
                    var logout = Convert.ToDateTime(attendanceGroup.Max(a => a.LogTime)).TimeOfDay;
                    //lateinMinute
                    var time1 = login.TotalMinutes > 10 * 60 ? Convert.ToInt32(login.TotalMinutes) - (10 * 60) : 0;
                    //EarlyOutMinute
                    var time2 = logout.Hours < 19 ? Convert.ToInt32((19 * 60) - logout.TotalMinutes) : 0;

                    var time3 = logout.TotalMinutes - login.TotalMinutes;

                    var split = (logout.Subtract(login)).ToString().Split(':');
                    //work hour
                    decimal work = Convert.ToDecimal(split[0].ToString() + "." + split[1].ToString());
                    //OverTimeMinute
                    var time4 = time3 > 540 ? Convert.ToInt32(time3 - 540) : 0;

                    var attLogsToProcess = attendanceGroup.Select(att => new AttAttendanceLog
                    {
                        UserId = att.UserId,
                        Username = att.Username,
                        LogTime = att.LogTime,
                        AttDate = att.AttDate,
                        Createdby = att.Createdby,
                        Createddate = att.Createddate
                    });

                    context.AttAttendanceLogs.RemoveRange(attendanceGroup);
                    context.AttAttendanceRegisters.Add(new AttAttendanceRegister
                    {
                        UserId = attendance.UserId,
                        UserName = attendance.Username,
                        AttDate = attendance.AttDate,
                        LateInMinute = time1,
                        EarlyOutMinute = time2,
                        WorkHour = work,
                        OverTimeMinute = time4,
                        IsLeave = null,
                        IsHoliday = null,
                        IsWeekOff = null,
                        ProcessDate = DateTime.Now,
                        ProcessBy = "Priyansh Soni"
                    });

                    context.AttAttendanceLogs.AddRange(attLogsToProcess);
                }

                await context.SaveChangesAsync();

                return RedirectToAction("ProcessDataCompleted", new { month = month, year = year });
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel
                {
                    ControllerName = this.RouteData.Values["controller"].ToString(),
                    ActionName = this.RouteData.Values["action"].ToString(),
                    ErrorMessage = ex.Message
                });
            }
        }

        public IActionResult ProcessDataCompleted(int month, int year)
        {
            return View(new ProcessDataCompletedViewModel { Month = month, Year = year });
        }

        public IActionResult CheckDataAvailability(int month, int year)
        {
            bool dataExists = CheckDataExistence(month, year);
            return Json(dataExists);
        }

        private bool CheckDataExistence(int month, int year)
        {
            var attAttendanceLogs = context.AttAttendanceLogs
                .Any(x => x.AttDate.Month == month && x.AttDate.Year == year);

            return attAttendanceLogs;
        }

        private bool IsRowEmpty(DataRow row)
        {
            foreach (var item in row.ItemArray)
            {
                if (!string.IsNullOrEmpty(item.ToString()))
                {
                    return false;
                }
            }
            return true;
        }

        
        public async Task<IActionResult> LateinMinutes(int month, int year)
        {
            var attAttendanceRegisters = await context.AttAttendanceRegisters
                .Where(x => x.AttDate.Month == month && x.AttDate.Year == year && x.LateInMinute > 0)
                .ToListAsync();

            // Group the attendance registers by UserId
            var groupedRegisters = attAttendanceRegisters.GroupBy(r => r.UserId);

            List<LateInReportModel> reportData = new List<LateInReportModel>();

            foreach (var group in groupedRegisters)
            {
                // Get the user details for the group
                var user = attAttendanceRegisters.FirstOrDefault(r => r.UserId == group.Key);

                var reportModel = new LateInReportModel
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    ReportEntries = new List<LateInReportEntry>(),
                    GroupTotal = TimeSpan.Zero,
                    LoginTime = TimeSpan.Zero,
                    LogoutTime = TimeSpan.Zero
                };

                foreach (var register in group)
                {
                    TimeSpan lateInMinute = TimeSpan.FromMinutes((double)(register.LateInMinute));

                    var attendanceLogs = await context.AttAttendanceLogs
                        .Where(r => r.UserId == user.UserId && r.AttDate.Date == register.AttDate.Date)
                        .OrderBy(r => r.LogTime)
                        .ToListAsync();

                    var loginTime = attendanceLogs.FirstOrDefault()?.LogTime.TimeOfDay;
                    var logoutTime = attendanceLogs.LastOrDefault()?.LogTime.TimeOfDay;

                    reportModel.ReportEntries.Add(new LateInReportEntry
                    {
                        AttDate = register.AttDate,
                        LateInMinute = lateInMinute,
                        LoginTime = loginTime ?? TimeSpan.Zero,
                        LogoutTime = logoutTime ?? TimeSpan.Zero
                    });

                    reportModel.GroupTotal += lateInMinute;
                    //reportModel.LoginTime = loginTime ?? TimeSpan.Zero;
                    //reportModel.LogoutTime = logoutTime ?? TimeSpan.Zero;
                }

                reportData.Add(reportModel);
            }

            ViewData["Year"] = year;
            ViewData["Month"] = month;

            return View("LateinMinutes", reportData);
        }


        public async Task<IActionResult> OverTimeMinute(int month, int year)
        {
            var attAttendanceRegisters = await context.AttAttendanceRegisters
                .Where(x => x.AttDate.Month == month && x.AttDate.Year == year && x.OverTimeMinute > 0)
                .ToListAsync();

            // Group the attendance registers by UserId
            var groupedRegisters = attAttendanceRegisters.GroupBy(r => r.UserId);

            List<OverTimeMinuteModel> reportData = new List<OverTimeMinuteModel>();

            foreach (var group in groupedRegisters)
            {
                // Get the user details for the group
                var user = attAttendanceRegisters.FirstOrDefault(r => r.UserId == group.Key);

                var reportModel = new OverTimeMinuteModel
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    ReportEntries = new List<OverTimeMinuteEntry>(),
                    GroupTotal = TimeSpan.Zero,
                    LoginTime = TimeSpan.Zero,
                    LogoutTime = TimeSpan.Zero
                };

                foreach (var register in group)
                {
                    TimeSpan overTimeMinute = TimeSpan.FromMinutes((double)(register.OverTimeMinute));

                    var attendanceLogs = await context.AttAttendanceLogs
                        .Where(r => r.UserId == user.UserId && r.AttDate.Date == register.AttDate.Date)
                        .OrderBy(r => r.LogTime)
                        .ToListAsync();

                    var loginTime = attendanceLogs.FirstOrDefault()?.LogTime.TimeOfDay;
                    var logoutTime = attendanceLogs.LastOrDefault()?.LogTime.TimeOfDay;

                    reportModel.ReportEntries.Add(new OverTimeMinuteEntry
                    {
                        AttDate = register.AttDate,
                        OverTimeMinute = overTimeMinute,
                        LoginTime = loginTime ?? TimeSpan.Zero,
                        LogoutTime = logoutTime ?? TimeSpan.Zero
                    });

                    reportModel.GroupTotal += overTimeMinute;
                    //reportModel.LoginTime = loginTime ?? TimeSpan.Zero;
                    //reportModel.LogoutTime = logoutTime ?? TimeSpan.Zero;
                }

                reportData.Add(reportModel);
            }

            ViewData["Year"] = year;
            ViewData["Month"] = month;

            return View("OverTimeMinute", reportData);
        }

        public async Task<IActionResult> EarlyOutMinute(int month, int year)
        {
            var attAttendanceRegisters = await context.AttAttendanceRegisters
                .Where(x => x.AttDate.Month == month && x.AttDate.Year == year && x.EarlyOutMinute > 0)
                .ToListAsync();

            // Group the attendance registers by UserId
            var groupedRegisters = attAttendanceRegisters.GroupBy(r => r.UserId);

            List<EarlyOutMinuteModel> reportData = new List<EarlyOutMinuteModel>();

            foreach (var group in groupedRegisters)
            {
                // Get the user details for the group
                var user = attAttendanceRegisters.FirstOrDefault(r => r.UserId == group.Key);

                var reportModel = new EarlyOutMinuteModel
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    ReportEntries = new List<EarlyOutMinuteEntry>(),
                    GroupTotal = TimeSpan.Zero,
                    LoginTime = TimeSpan.Zero,
                    LogoutTime = TimeSpan.Zero
                };

                foreach (var register in group)
                {
                    TimeSpan earlyOutMinute = TimeSpan.FromMinutes((double)(register.EarlyOutMinute));

                    var attendanceLogs = await context.AttAttendanceLogs
                        .Where(r => r.UserId == user.UserId && r.AttDate.Date == register.AttDate.Date)
                        .OrderBy(r => r.LogTime)
                        .ToListAsync();

                    var loginTime = attendanceLogs.FirstOrDefault()?.LogTime.TimeOfDay;
                    var logoutTime = attendanceLogs.LastOrDefault()?.LogTime.TimeOfDay;

                    reportModel.ReportEntries.Add(new EarlyOutMinuteEntry
                    {
                        AttDate = register.AttDate,
                        EarlyOutMinute = earlyOutMinute,
                        LoginTime = loginTime ?? TimeSpan.Zero,
                        LogoutTime = logoutTime ?? TimeSpan.Zero
                    });

                    reportModel.GroupTotal += earlyOutMinute;
                    //reportModel.LoginTime = loginTime ?? TimeSpan.Zero;
                    //reportModel.LogoutTime = logoutTime ?? TimeSpan.Zero;
                }

                reportData.Add(reportModel);
            }

            ViewData["Year"] = year;
            ViewData["Month"] = month;

            return View("EarlyOutMinute", reportData);
        }

        public async Task<IActionResult> WorkHourReport(int month, int year)
        {
            var attAttendanceRegisters = await context.AttAttendanceRegisters
                .Where(x => x.AttDate.Month == month && x.AttDate.Year == year && x.WorkHour > 0)
                .ToListAsync();

            // Group the attendance registers by UserId
            var groupedRegisters = attAttendanceRegisters.GroupBy(r => r.UserId);

            List<WorkHourReportModel> reportData = new List<WorkHourReportModel>();

            foreach (var group in groupedRegisters)
            {
                // Get the user details for the group
                var user = attAttendanceRegisters.FirstOrDefault(r => r.UserId == group.Key);

                var reportModel = new WorkHourReportModel
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    ReportEntries = new List<WorkHourReportEntry>(),
                    GroupTotal = TimeSpan.Zero,
                    LoginTime = TimeSpan.Zero,
                    LogoutTime = TimeSpan.Zero
                };

                foreach (var register in group)
                {
                    TimeSpan workHour = TimeSpan.FromMinutes((double)(register.WorkHour * 60));

                    var attendanceLogs = await context.AttAttendanceLogs
                        .Where(r => r.UserId == user.UserId && r.AttDate.Date == register.AttDate.Date)
                        .OrderBy(r => r.LogTime)
                        .ToListAsync();

                    var loginTime = attendanceLogs.FirstOrDefault()?.LogTime.TimeOfDay;
                    var logoutTime = attendanceLogs.LastOrDefault()?.LogTime.TimeOfDay;

                    reportModel.ReportEntries.Add(new WorkHourReportEntry
                    {
                        AttDate = register.AttDate,
                        WorkHour = workHour,
                        LoginTime = loginTime ?? TimeSpan.Zero,
                        LogoutTime = logoutTime ?? TimeSpan.Zero
                    });

                    reportModel.GroupTotal += workHour;
                    //reportModel.LoginTime = loginTime ?? TimeSpan.Zero;
                    //reportModel.LogoutTime = logoutTime ?? TimeSpan.Zero;
                }

                reportData.Add(reportModel);
            }

            ViewData["Year"] = year;
            ViewData["Month"] = month;

            return View("WorkHourReport", reportData);
        }
     }
}
