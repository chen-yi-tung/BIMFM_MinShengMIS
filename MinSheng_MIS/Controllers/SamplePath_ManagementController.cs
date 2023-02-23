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
        public ActionResult Management()
        {
            return View();
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
        public ActionResult Update()
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