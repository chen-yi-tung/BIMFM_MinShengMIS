using MinSheng_MIS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        [HttpPost]
        public ActionResult Management_datagrid(FormCollection form) {
            JObject jo = new JObject();
            var service = new DatagridService();
            var a = service.GetJsonForGrid_ReportManagement(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }

        #endregion
        #region 報修管理詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion
    }
}