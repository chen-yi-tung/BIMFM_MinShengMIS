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

        #region 巡檢保養紀錄詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 巡檢保養紀錄審核
        public ActionResult Audit()
        {
            return View();
        }
        #endregion

        #region 巡檢保養紀錄補件
        public ActionResult supplement()
        {
            return View();
        }
        #endregion
    }
}