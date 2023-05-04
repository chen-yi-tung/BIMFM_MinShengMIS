using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class PurchaseOrder_ManagementController : Controller
    {
        // GET: PurchaseOrder_Management
        #region 採購管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 採購詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 採購收貨
        public ActionResult Delivery()
        {
            return View();
        }
        #endregion

        #region 採購驗收
        public ActionResult Accept()
        {
            return View();
        }
        #endregion
    }
}