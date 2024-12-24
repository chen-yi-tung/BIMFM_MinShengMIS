using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class SampleSchedule_ManagementController : Controller
    {

        #region 每日巡檢時程模板管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 每日巡檢時程模板
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 編輯 每日巡檢時程模板
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 每日巡檢時程模板 詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion

        #region 每日巡檢時程模板 刪除
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}