using MinSheng_MIS.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class DatagridController : Controller
    {
        // GET: Datagrid
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Report_Management(FormCollection form)
        {
            string page = form["page"]?.ToString();
            string rows = form["rows"]?.ToString();
            JObject jo = new JObject();
            var service = new DatagridService();
            var a = service.GetJsonForGrid_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
    }
}