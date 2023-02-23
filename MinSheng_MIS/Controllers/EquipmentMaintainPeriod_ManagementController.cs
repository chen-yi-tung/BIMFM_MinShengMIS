using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentMaintainPeriod_ManagementController : Controller
    {
        #region 設備保養週期管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 設備保養週期管理編輯
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

    }
}