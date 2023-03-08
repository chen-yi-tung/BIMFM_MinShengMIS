using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
namespace MinSheng_MIS.Controllers
{
    public class Report_ManagementController : Controller
    {
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
    }
}