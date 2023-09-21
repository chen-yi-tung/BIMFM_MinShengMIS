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
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System.IO;

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
                List<ReportData> data = mr.Select(s => new ReportData { MRSN = s.MRSN, ReportTitle = s.ReportTitle, UploadUserName = s.UploadUserName, UploadDateTime = s.UploadDateTime.ToString("yyyy/MM/dd"), ReportContent = s.ReportContent, YearMonth = $"{s.Year}-{s.Month}" }).ToList();

                return Content(JsonConvert.SerializeObject(data), "application/json");
            }
            catch (Exception ex)
            {
                return Content(JsonConvert.SerializeObject(new List<ReportData> { new ReportData { ReportTitle = ex.Message, YearMonth = "ERROR" } }), "application/json");
            }

        }


        #region 新增月報
        public ActionResult Create()
        {
            return View();
        }


        public class CreateData
        {
            public string ReportTitle { get; set; }
            public string YearMonth { get; set; }
            public string ReportContent { get; set; }
            public HttpPostedFileBase ReportFile { get; set; }
        }

        [HttpPost]
        public ActionResult CreateMonthlyReport(CreateData createData)
        {
            try
            {
                string[] yearMonthParts = createData.YearMonth.Split('-');
                if (yearMonthParts.Length != 2) return Content("YearMonth ERROR", "application/json");

                Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
                string lastMRSN = db.MonthlyReport.OrderByDescending(mr => mr.MRSN).FirstOrDefault().MRSN;
                string lastMrDate = lastMRSN.Substring(0, 6);
                string dateNow = DateTime.Now.ToString("yyMMdd");
                int parsedValue = 1;
                if (lastMrDate == dateNow)
                {
                    string lastMrSerial = lastMRSN.Substring(6);
                    if (int.TryParse(lastMrSerial, out parsedValue) == false) return Content($" lastMrDate is {lastMrDate} and lastMrSerial is {lastMrSerial}  Failed to parse as an integer", "application/json");
                    parsedValue++;
                    lastMRSN = dateNow + parsedValue.ToString("00");
                }
                else lastMRSN = dateNow + parsedValue.ToString("00");


                string fileName = "";
                if (createData.ReportFile != null && createData.ReportFile.ContentLength > 0)
                {
                    string folderPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "Files", "MonthlyReport");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                    fileName = lastMRSN + Path.GetExtension(createData.ReportFile.FileName);
                    createData.ReportFile.SaveAs(Path.Combine(folderPath, fileName));
                }

                MonthlyReport newReport = new MonthlyReport
                {
                    ReportTitle = createData.ReportTitle,
                    ReportContent = createData.ReportContent,
                    Year = yearMonthParts[0],
                    Month = yearMonthParts[1],
                    MRSN = lastMRSN,
                    ReportFile = fileName,
                    UploadDateTime = DateTime.Now,
                    UploadUserName = User.Identity.Name,
                };
                db.MonthlyReport.Add(newReport);
                db.SaveChanges();
                return Content(JsonConvert.SerializeObject(new JObject { { "Succeed", true } }), "application/json");
            }
            catch (Exception ex)
            {
                return Content(ex.Message, "application/json");
            }
        }


        #endregion

        #region 編輯月報
        public ActionResult Edit(string id = "")
        {
            ViewBag.id = id;
            Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
            var mr = db.MonthlyReport.Where(m => m.MRSN == id).FirstOrDefault();
            return View();
        }
        #endregion



        [HttpGet]
        public ActionResult Readbody(string id)
        {
            JObject jo = new JObject();
            Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
            var item = db.MonthlyReport.Find(id);
            if (item == null ) return Content(JsonConvert.SerializeObject(new JObject { { "Failed", false } }), "application/json");
            jo["MRSN"] = item.MRSN;
            jo["ReportTitle"] = item.ReportTitle;
            jo["UploadUserName"] = item.UploadUserName;
            jo["UploadDateTime"] = item.UploadDateTime.ToString("yyyy/M/d"); ;
            jo["ReportContent"] = item.ReportContent;
            jo["YearMonth"] = item.Year + "-" + item.Month;
            jo["FilePath"] = string.IsNullOrEmpty(item.ReportFile) ? null : "\\Files\\MonthlyReport\\" + item.ReportFile;
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }











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