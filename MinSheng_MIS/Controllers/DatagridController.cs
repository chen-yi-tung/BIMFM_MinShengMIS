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

namespace MinSheng_MIS.Controllers
{
    public class DatagridController : Controller
    {
        // GET: Datagrid
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Report_Management(FormCollection form) //報修管理
        {
            string page = form["page"]?.ToString();
            string rows = form["rows"]?.ToString();
            JObject jo = new JObject();
            var service = new DatagridService();
            var a = service.GetJsonForGrid_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        [HttpPost]
        public ActionResult MaintainRecord_Management(FormCollection form) //巡檢保養紀錄管理
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_MaintainRecord_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        [HttpPost]
        public ActionResult RepairRecord_Management(FormCollection form) //巡檢維修紀錄管理
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_RepairRecord_Management(form); 
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        [HttpPost]
        public ActionResult EquipmentMaintainPeriod_Management(FormCollection form)
        {
            JObject jo = new JObject();
            var service = new DatagridService();
            var a = service.GetJsonForGrid_EquipmentMaintainPeriod_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        [HttpPost]
        public ActionResult Account_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_Account_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        [HttpPost]
        public ActionResult InspectationPlan_Record_EquipMaintain(FormCollection form)
        {
            var service = new DatagridService();
            string result = service.GetJsonForGrid_InspectationPlan_Record_EquipMaintain(form);
            return Content(result, "application/json");
        }

        [HttpPost]
        public ActionResult InspectationPlan_Record_EquipRepair(FormCollection form)
        {
            var service = new DatagridService();
            string result = service.GetJsonForGrid_InspectationPlan_Record_EquipRepair(form);
            return Content(result, "application/json");
        }
    }
}