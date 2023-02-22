using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class Report_ManagementController : Controller
    {
        // GET: Report_Management
        public ActionResult Index()
        {
            return View();
        }

        #region 報修管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 報修管理詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion
    }
}