using MinSheng_MIS.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Models;
using System.Web.Services.Description;

namespace MinSheng_MIS.Controllers
{
    public class DatagridController : Controller
    {
        #region Report_Management 報修管理
        [HttpPost]
        public ActionResult Report_Management(FormCollection form)
        {
            string page = form["page"]?.ToString();
            string rows = form["rows"]?.ToString();
            JObject jo = new JObject();
            var service = new DatagridService();
            var a = service.GetJsonForGrid_Report_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region MaintainRecord_Management 巡檢保養紀錄管理
        [HttpPost]
        public ActionResult MaintainRecord_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_MaintainRecord_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region RepairRecord_Management 巡檢維修紀錄管理
        [HttpPost]
        public ActionResult RepairRecord_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_RepairRecord_Management(form); 
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region MaintainItem_Management 保養項目管理
        [HttpPost]
        public ActionResult MaintainItem_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_MaintainItem(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion
        
        #region EquipmentMaintainPeriod_Management 設備保養週期管理
        [HttpPost]
        public ActionResult EquipmentMaintainPeriod_Management(FormCollection form)
        {
            JObject jo = new JObject();
            var service = new DatagridService();
            var a = service.GetJsonForGrid_EquipmentMaintainPeriod_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region Account_Management 帳號管理
        [HttpPost]
        public ActionResult Account_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_Account_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region InspectationPlan_Record_EquipMaintain 巡檢紀錄_設備保養紀錄
        [HttpPost]
        public ActionResult InspectationPlan_Record_EquipMaintain(FormCollection form)
        {
            var service = new DatagridService();
            string result = service.GetJsonForGrid_InspectationPlan_Record_EquipMaintain(form);
            return Content(result, "application/json");
        }
        #endregion

        #region InspectationPlan_Record_EquipRepair 巡檢紀錄_設備維修紀錄
        [HttpPost]
        public ActionResult InspectationPlan_Record_EquipRepair(FormCollection form)
        {
            var service = new DatagridService();
            string result = service.GetJsonForGrid_InspectationPlan_Record_EquipRepair(form);
            return Content(result, "application/json");
        }
        #endregion

        #region AsBuiltDrawing_Management 竣工圖說管理
        [HttpPost]
        public ActionResult AsBuiltDrawing_Management(FormCollection form)
        {
            var service = new DatagridService();
            string result = service.GetJsonForGrid_AsBuiltDrawing(form);
            return Content(result, "application/json");
        }
        #endregion
    }
}