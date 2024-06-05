using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
namespace MinSheng_MIS.Controllers
{
    public class Report_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 報修管理
        public ActionResult Management()
        {
            return View();
        }
        //[HttpPost]
        //public ActionResult Management_datagrid(FormCollection form) {
        //    string page = form["page"]?.ToString();
        //    string rows = form["rows"]?.ToString();
        //    JObject jo = new JObject();
        //    var service = new DatagridService();
        //    var a = service.GetJsonForGrid_Management(form);
        //    string result = JsonConvert.SerializeObject(a);
        //    return Content(result, "application/json");
        //}

        #endregion

        #region 報修管理詳情
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult ReadBody(string id) 
        {
            var reportManagementViewModel = new ReportManagementViewModel();
            
            string result = reportManagementViewModel.GetJsonForRead(id);
            return Content(result, "application/json");
        }
        #endregion

        #region 報修單取消保留
        [HttpPost]
        public ActionResult Cancel(string id)
        {
            //取消保留
            var ReportForm = db.EquipmentReportForm.Find(id);
            switch (ReportForm.ReportState)
            {
                case "9":
                    ReportForm.ReportState = "1";
                    break;
                case "10":
                    ReportForm.ReportState = "5";
                    break;
                case "11":
                    ReportForm.ReportState = "8";
                    break;
            }
            db.EquipmentReportForm.AddOrUpdate(ReportForm);
            db.SaveChanges();

            JObject jo = new JObject();
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增報修
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 報修匯出
        [HttpPost]
        public ActionResult Export(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_Report_Management(form);
            string ctrlName = this.ControllerContext.RouteData.Values["controller"].ToString();
            var result = ComFunc.ExportExcel(Server, a["rows"], ctrlName);

            return Json(result);
        }
        #endregion
    }
}