using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models;

namespace MinSheng_MIS.Controllers
{
    public class MonthlyReport_ManagementController : Controller
    {
        // GET: MonthlyReport_Management

        #region 月報管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion


        [HttpPost]
        public ActionResult GetData()
        {
            try
            {
                Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
                var mr = db.MonthlyReport.ToList();

                List<ReportData> data = mr
                .Select(s => new ReportData
                {
                    MRSN = s.MRSN,
                    ReportTitle = s.ReportTitle,
                    UploadUserName = s.UploadUserName,
                    UploadDateTime = s.UploadDateTime.ToString("yyyy/MM/dd"), // Convert DateTime to string
                    ReportContent = s.ReportContent,
                    YearMonth = s.Year + "-" + s.Month // Combine Year and Month
                })
                .ToList();


                List<ReportData> data2 = mr.Select(s => new ReportData
                {
                    MRSN = s.MRSN,
                    ReportTitle = s.ReportTitle,
                    UploadUserName = s.UploadUserName,
                    UploadDateTime = s.UploadDateTime.ToString("yyyy/MM/dd"), // Convert DateTime to string format
                    ReportContent = s.ReportContent,
                    YearMonth = $"{s.Year}-{s.Month}" // Combine Year and Month into YearMonth
                }).ToList();


                List<ReportData> fakeData = new List<ReportData>
                {
                    new ReportData
                    {
                        MRSN = "1",
                        ReportTitle = "2023-5月月報",
                        UploadUserName = "王大明",
                        UploadDateTime = "2023/5/22",
                        ReportContent = "OOXX",
                        YearMonth = "2023-05"
                    },
                    new ReportData
                    {
                        MRSN = "2",
                        ReportTitle = "2023-6月月報",
                        UploadUserName = "林小美",
                        UploadDateTime = "2023/6/15",
                        ReportContent = "XYZ",
                        YearMonth = "2023-06"
                    },
                    new ReportData
                    {
                        MRSN = "3",
                        ReportTitle = "2023-7月月報",
                        UploadUserName = "陳大雄",
                        UploadDateTime = "2023/7/20",
                        ReportContent = "ABCD",
                        YearMonth = "2023-07"
                    }
                };

                return Content(JsonConvert.SerializeObject(data2), "application/json");



                //string P_Name = "", F_Type = "", P_F_Date = "", P_F_DateEnd = "", Keyword = "";
                //string yourValue = TempData["TargetStr"] as string;
                //if (Query != null && yourValue != null)
                //{
                //    string QT = yourValue.Trim();
                //    QT = QT.Replace("&quot;", "\"");
                //    var jsonString = QT;
                //    dynamic Target = JValue.Parse(jsonString);
                //    P_Name = Target.P_Name;
                //    F_Type = Target.F_Type;
                //    P_F_Date = Target.P_F_Date;
                //    P_F_DateEnd = Target.P_F_DateEnd;
                //    Keyword = Target.Keyword;
                //}
                //var allFm = await _context.FileManagement.Where(f => f.P_Name != "9").ToListAsync();
                //if (!string.IsNullOrEmpty(P_Name)) allFm = allFm.Where(a => a.P_Name.ToLower().Contains(P_Name.ToLower())).ToList();
                //if (!string.IsNullOrEmpty(F_Type)) allFm = allFm.Where(a => a.F_Type.ToLower().Contains(F_Type.ToLower())).ToList();
                //if (!string.IsNullOrEmpty(Keyword)) allFm = allFm.Where(a => a.P_FileName.ToLower().Contains(Keyword.ToLower())).ToList();
                //DateTime P_S_DateDT, P_E_DateDT;
                //bool isP_S_DateValid = DateTime.TryParseExact(P_F_Date, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out P_S_DateDT);
                //bool isP_E_DateValid = DateTime.TryParseExact(P_F_DateEnd, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out P_E_DateDT);
                //if (isP_S_DateValid && isP_E_DateValid) { allFm = allFm.Where(a => a.P_F_Date >= P_S_DateDT && a.P_F_Date <= P_E_DateDT).ToList(); }
                //else if (isP_S_DateValid) { allFm = allFm.Where(a => a.P_F_Date >= P_S_DateDT).ToList(); }
                //else if (isP_E_DateValid) { allFm = allFm.Where(a => a.P_F_Date <= P_E_DateDT).ToList(); }

                //int totalCount = allFm.Count;
                //allFm = allFm.Skip((page - 1) * rows).Take(rows).ToList();
                //var pmRow = new List<FileManagmentJSON.Row>();
                //var allParam = await _context.CustomizedParameterTypeManagement.ToListAsync();
                //if (User.Identity!.Name == "superadmin@gmail.com")
                //{
                //    pmRow = allFm.Select(item => new FileManagmentJSON.Row
                //    {
                //        id = item.Id.ToString() ?? "",
                //        btnbool = "1" ?? "",
                //        btn2bool = "1" ?? "",
                //        Button2 = "編輯",
                //        Button3 = "下載",
                //        P_ID = item.P_ID,
                //        P_Name = item.P_Name,
                //        F_Type = (allParam.Where(c => c.ParamType_ID == item.F_Type).FirstOrDefault()).ParamType_Name ?? "",
                //        P_F_Name = item.P_FileName,// _DBService.NormalizeFileName(item.P_FileName ?? ""),
                //        P_F_Date = (item.P_F_Date == null) ? "" : Convert.ToDateTime(item.P_F_Date.ToString()).ToString("yyyy/MM/dd"),
                //    }).ToList();
                //    return Content(JsonConvert.SerializeObject(new { total = totalCount, rows = pmRow }), "application/json");
                //}


                //List<AspNetRoleClaimsTb> aspNetRoleClaimsTbs = new List<AspNetRoleClaimsTb>();

                //var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                //if (userid != null && userid != "")
                //{
                //    var atheUser = await _context.UserRoles.Where(a => a.UserId == userid).FirstOrDefaultAsync();
                //    if (atheUser != null)
                //    {
                //        aspNetRoleClaimsTbs = await _context.AspNetRoleClaimsTb.Where(a => a.RoleId == atheUser.RoleId).ToListAsync();
                //        bool hasClaimValue = aspNetRoleClaimsTbs.Any(u => u.ClaimValue == "Permissions.SystemManagements");

                //    }
                //}


                //pmRow = allFm.Select(item => new FileManagmentJSON.Row
                //{
                //    id = item.Id.ToString(),
                //    //btnbool = ((_authorizationService.AuthorizeAsync(User, "Permissions.File.Edit." + item.P_ID)).Result.Succeeded) ? "1" : "0",
                //    //btn2bool = ((_authorizationService.AuthorizeAsync(User, "Permissions.File.Download." + item.P_ID)).Result.Succeeded) ? "1" : "0",
                //    btnbool = aspNetRoleClaimsTbs.Any(u => u.ClaimValue == "Permissions.File.Edit." + item.P_ID) ? "1" : "0",
                //    btn2bool = aspNetRoleClaimsTbs.Any(u => u.ClaimValue == "Permissions.File.Download." + item.P_ID) ? "1" : "0",
                //    Button2 = "編輯",
                //    Button3 = "下載",
                //    P_ID = item.P_ID,
                //    P_Name = item.P_Name,
                //    F_Type = (allParam.Where(c => c.ParamType_ID == item.F_Type).FirstOrDefault()).ParamType_Name ?? "",
                //    P_F_Name = item.P_FileName,// _DBService.NormalizeFileName(item.P_FileName ?? ""),
                //    P_F_Date = (item.P_F_Date == null) ? "" : Convert.ToDateTime(item.P_F_Date.ToString()).ToString("yyyy/MM/dd"),
                //}).ToList();

                //pmRow.RemoveAll(row => row.btnbool == "0" && row.btn2bool == "0");

                //var resultPM = new { total = totalCount, rows = pmRow };
                return null;
                //return Content(JsonConvert.SerializeObject(resultPM), "application/json");
            }
            catch (Exception ex)
            {
                return null;
                //FileManagmentJSON.Row errorRow = new() { Button2 = "", Button3 = "", P_ID = ex.Message ?? "Error", P_Name = ex.StackTrace ?? "Error", F_Type = "", P_F_Name = "", P_F_Date = "" };
                //FileManagmentJSON.Rootobject result = new() { total = 1, rows = new[] { errorRow } };
                //return Content(JsonConvert.SerializeObject(result), "application/json");
            }

        }







        #region 新增月報
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 編輯月報
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 月報詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 刪除月報
        public ActionResult Delete()
        {
            return View();
        }
        #endregion


        public class ReportData
        {
            public string MRSN { get; set; }
            public string ReportTitle { get; set; }
            public string UploadUserName { get; set; }
            public string UploadDateTime { get; set; }
            public string ReportContent { get; set; }
            public string YearMonth { get; set; }
        }

    }
}