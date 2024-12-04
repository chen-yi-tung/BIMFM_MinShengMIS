using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentOperatingManualController : Controller
    {
        #region 操作手冊
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 設備操作手冊
        public ActionResult Create()
        {
            return View();
        }
        #endregion

    }
}