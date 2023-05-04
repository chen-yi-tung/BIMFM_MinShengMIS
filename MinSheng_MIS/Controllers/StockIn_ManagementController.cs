using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class StockIn_ManagementController : Controller
    {
        // GET: StockIn_Management
        #region 入庫管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增入庫
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 入庫詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion
    }
}