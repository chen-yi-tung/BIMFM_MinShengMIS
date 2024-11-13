using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class SamplePath_ManagementController : Controller
    {
        #region 巡檢路線模板管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 巡檢路線模板
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 編輯 巡檢路線模板
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 巡檢路線模板 詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion

        #region 巡檢路線模板 刪除
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}