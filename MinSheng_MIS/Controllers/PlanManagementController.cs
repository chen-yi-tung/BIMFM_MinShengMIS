using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class PlanManagementController : Controller
    {
        #region 工單管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增工單
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 編輯工單
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 工單詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion
    }
}