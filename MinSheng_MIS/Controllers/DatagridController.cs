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
        MaintainRecordManagementModel MRMM = new MaintainRecordManagementModel();

        // GET: Datagrid
        [HttpGet]
        public ActionResult Index()
        {
            //ViewModel傳前端下拉選項
            MaintainRecordManagementViewModel.Management management = new MaintainRecordManagementViewModel.Management()
            { 
                AreaList = MRMM.GetAreaList(),
                FloorList= MRMM.GetFloorList(),
                MaintainStateList= MRMM.GetMaintainStateList(),
                ESNList = MRMM.GetESNList(),
                ENameList= MRMM.GetENameList(),
                MaintainUserIDList= MRMM.GetMaintainUserList(),
                AuditUserIDList= MRMM.GetAuditUserList()
            };
            return View(management);
        }
        [HttpPost]
        public ActionResult Report_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_Management(form);
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
    }
}