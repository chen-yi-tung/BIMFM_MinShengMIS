using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class StockOut_ManagementController : Controller
    {
        // GET: StockOut_Management
        #region 出庫管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增出庫
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 出庫詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion
    }
}