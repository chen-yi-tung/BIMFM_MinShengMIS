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
    public class SamplePath_ManagementController : Controller
    {
        SamplePath_DataService SP_ds = new SamplePath_DataService();

        #region 巡檢路線模板管理
        public ActionResult Management()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SamplePath_Management(FormCollection form)
        {
            var a = SP_ds.GetJsonForGrid_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增巡檢路線模板
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 巡檢路線模板詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯巡檢路線模板
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除巡檢路線模板
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}