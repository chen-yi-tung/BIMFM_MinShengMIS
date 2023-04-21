using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class ManufacturerInfo_ManagementController : Controller
    {
        DatagridService ds = new DatagridService();
        // GET: ManufacturerInfo_Management
        #region 廠商管理
        public ActionResult Management()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ManufacturerInfo_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = ds.GetJsonForGrid_ManufacturerInfo_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增廠商
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 廠商詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯廠商
        public ActionResult Edit()
        {
            return View();
        }
        #endregion
    }
}