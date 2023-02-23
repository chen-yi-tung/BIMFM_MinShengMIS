using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class MaintainForm_ManagementController : Controller
    {
        // GET: MaintainForm_Management
        public ActionResult Index()
        {
            return View();
        }

        #region 定期保養管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 定期保養詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion
    }
}