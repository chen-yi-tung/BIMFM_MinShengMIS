using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class MaintainRecord_ManagementController : Controller
    {

        #region 巡檢保養紀錄管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 查詢巡檢保養紀錄 (詳情)
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpGet]
        public ActionResult ReadBody(string id)
        {
            var MaintainRecord_Management_ViewModel = new MaintainRecord_Management_ViewModel();

            string result = MaintainRecord_Management_ViewModel.GetJsonForRead(id);
            return Content(result, "application/json");
        }
        #endregion

        #region 巡檢保養紀錄審核
        public ActionResult Audit()
        {
            return View();
        }
        #endregion

        #region 巡檢保養紀錄補件
        public ActionResult Supplement()
        {
            return View();
        }
        #endregion
    }
}