using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class Stock_ManagementController : Controller
    {
        // GET: Stock_Management
        #region 庫存管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 庫存詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 庫存警戒值設定
        public ActionResult MinStockAmountSetting()
        {
            return View();
        }
        #endregion
    }
}