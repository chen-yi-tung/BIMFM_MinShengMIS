using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class PurchaseRequisition_ManagementController : Controller
    {
        // GET: PurchaseRequisition_Management
        #region 請購管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 請購申請
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 請購審核
        public ActionResult Audit()
        {
            return View();
        }
        #endregion
    }
}