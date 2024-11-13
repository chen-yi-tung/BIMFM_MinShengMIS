using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class Repair_ManagementController : Controller
    {
        #region 報修管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 報修單
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 報修單 審核
        public ActionResult Review()
        {
            return View();
        }
        #endregion

        #region 報修單 詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion
    }
}