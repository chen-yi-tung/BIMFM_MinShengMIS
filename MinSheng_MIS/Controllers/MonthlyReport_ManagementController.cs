using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models;
using System.IO;
using MinSheng_MIS.Services;

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

        [HttpPost]
        public ActionResult GetData(FormCollection form)
        {
            try
            {
                int page = 1;
                if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
                int rows = 10;
                if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());
                string reportTitle = form["ReportTitle"]?.ToString();
                string uploadUserName = form["UploadUserName"]?.ToString();

                string dateFrom = form["DateFrom"]?.ToString();
                string dateTo = form["DateTo"]?.ToString();

                Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

                var mr = from x in db.MonthlyReport
                         select new { x.ReportTitle, x.UploadUserName, YearMonth = x.Year + "-" + x.Month, x.MRSN, x.ReportContent, x.UploadDateTime };

                if (!string.IsNullOrEmpty(reportTitle)) mr = mr.Where(x => x.ReportTitle.Contains(reportTitle));
                if (!string.IsNullOrEmpty(uploadUserName)) mr = mr.Where(x => x.UploadUserName.Contains(uploadUserName));

                if (!string.IsNullOrEmpty(dateFrom))
                {
                    var datestart = DateTime.Parse(dateFrom);
                    mr = mr.ToList().Where(x => DateTime.ParseExact(x.YearMonth, "yyyy-MM", CultureInfo.InvariantCulture) >= datestart).AsQueryable();
                }

                if (!string.IsNullOrEmpty(dateTo))
                {
                    var dateend = DateTime.Parse(dateTo).AddMonths(1); // Add one month to include records up to the end of the specified month
                    mr = mr.ToList().Where(x => DateTime.ParseExact(x.YearMonth, "yyyy-MM", CultureInfo.InvariantCulture) < dateend).AsQueryable();
                }

                #region datagrid remoteSort 判斷有無 sort 跟 order
                IValueProvider vp = form.ToValueProvider();
                if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
                {
                    string sort = form["sort"]?.ToString();
                    string order = form["order"]?.ToString();

                    if (order == "asc")
                    {
                        mr = DatagridService.OrderByField(mr, sort, true);
                    }
                    else if (order == "desc")
                    {
                        mr = DatagridService.OrderByField(mr, sort, false);
                    }
                }
                else
                {
                    mr = mr.OrderByDescending(x => x.MRSN);
                }
                #endregion

                JArray ja = new JArray();
                int total = mr.Count();
                mr = mr.Skip((page - 1) * rows).Take(rows);
                foreach (var item in mr)
                {
                    var itemObjects = new JObject
                    {
                        { "MRSN", item.MRSN },
                        { "ReportTitle", item.ReportTitle },
                        { "UploadUserName", item.UploadUserName },
                        { "ReportContent", item.ReportContent },
                        { "YearMonth", item.YearMonth },
                        { "UploadDateTime", (item.UploadDateTime != DateTime.MinValue && item.UploadDateTime != null) ? item.UploadDateTime.ToString("yyyy/MM/dd") : null }
                    };
                    ja.Add(itemObjects);
                }
                JObject jo = new JObject();
                jo.Add("rows", ja);
                jo.Add("total", total);
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }
            catch (Exception ex)
            {
                return Content(JsonConvert.SerializeObject(new List<ReportData> { new ReportData { ReportTitle = ex.Message, YearMonth = "ERROR" } }), "application/json");
            }

        }

        #endregion

        #region 新增月報
        public ActionResult Create()
        {
            return View();
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

                MonthlyReport newReport = new MonthlyReport { ReportTitle = createData.ReportTitle, ReportContent = createData.ReportContent, Year = yearMonthParts[0], Month = yearMonthParts[1], MRSN = lastMRSN, ReportFile = fileName, UploadDateTime = DateTime.Now, UploadUserName = User.Identity.Name, };
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
            return View();
        }

        public ActionResult EditMonthlyReport(CreateData createData)
        {
            if (string.IsNullOrEmpty(createData.YearMonth)) return Json(new { success = false, message = "Item not found" });
            string[] parts = createData.YearMonth.Split('-');

            Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
            var item = db.MonthlyReport.Find(createData.MRSN);
            if (item != null)
            {
                item.ReportTitle = createData.ReportTitle;
                item.ReportContent = createData.ReportContent;
                item.UploadUserName = User.Identity.Name;
                item.UploadDateTime = DateTime.Now;
                item.Year = parts[0];
                item.Month = parts[1];
                if (createData.ReportFile != null && createData.ReportFile.ContentLength > 0)
                {
                    string folderPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "Files", "MonthlyReport");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                    item.ReportFile = item.MRSN + Path.GetExtension(createData.ReportFile.FileName);
                    createData.ReportFile.SaveAs(Path.Combine(folderPath, item.ReportFile));
                }
                db.SaveChanges();
                return Json(new { success = true });
            }
            else return Json(new { success = false, message = "Item not found" });

        }
        #endregion

        #region 月報詳情
        public ActionResult Read(string id = "")
        {
            ViewBag.id = id;
            return View();
        }
        #endregion

        #region 刪除月報
        public ActionResult Delete(string id = "")
        {
            ViewBag.id = id;
            return View();
        }
        public ActionResult Delete_MonthlyReport(CreateData createData, string MRSN = "")
        {
            try
            {
                Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
                var item = db.MonthlyReport.Find(MRSN);
                if (item != null)
                {
                    db.MonthlyReport.Remove(item); // Mark the item for deletion
                    db.SaveChanges(); // Persist the deletion to the database
                    return Json(new { success = true });// Return a success JSON response
                }
                else return Json(new { success = false, message = "Item not found" }); // Return an error JSON response

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });// Return an error JSON response with the exception message
            }
        }

        #endregion

        [HttpGet]
        public ActionResult Readbody(string id)
        {
            JObject jo = new JObject();
            Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
            var item = db.MonthlyReport.Find(id);
            if (item == null) return new HttpNotFoundResult("optional description"); //return Content(JsonConvert.SerializeObject(new JObject { { "Failed", false } }), "application/json");
            jo["MRSN"] = item.MRSN;
            jo["ReportTitle"] = item.ReportTitle;
            jo["UploadUserName"] = item.UploadUserName;
            jo["UploadDateTime"] = item.UploadDateTime.ToString("yyyy/M/d"); ;
            jo["ReportContent"] = item.ReportContent;
            jo["YearMonth"] = item.Year + "-" + item.Month;
            jo["FilePath"] = string.IsNullOrEmpty(item.ReportFile) ? null : "/Files/MonthlyReport/" + item.ReportFile;
            jo["FileName"] = string.IsNullOrEmpty(item.ReportFile) ? null : item.ReportFile;
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }

        public class ReportData
        {
            public string MRSN { get; set; }
            public string ReportTitle { get; set; }
            public string UploadUserName { get; set; }
            public string UploadDateTime { get; set; }
            public string ReportContent { get; set; }
            public string YearMonth { get; set; }
        }

        public class CreateData
        {
            public string ReportTitle { get; set; }
            public string YearMonth { get; set; }
            public string ReportContent { get; set; }
            public HttpPostedFileBase ReportFile { get; set; }
            public string ReportFileStr { get; set; }
            public string MRSN { get; set; }
        }
    }
}