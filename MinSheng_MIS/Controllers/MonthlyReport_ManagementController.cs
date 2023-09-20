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
                List<ReportData> data = mr.Select(s => new ReportData { MRSN = s.MRSN, ReportTitle = s.ReportTitle, UploadUserName = s.UploadUserName, UploadDateTime = s.UploadDateTime.ToString("yyyy/MM/dd"), ReportContent = s.ReportContent, YearMonth = $"{s.Year}-{s.Month}" }).ToList();



                return Content(JsonConvert.SerializeObject(data), "application/json");

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