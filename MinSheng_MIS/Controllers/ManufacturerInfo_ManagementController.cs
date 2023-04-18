using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class ManufacturerInfo_ManagementController : Controller
    {
        // GET: ManufacturerInfo_Management
        #region 廠商管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增廠商
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 廠商詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯廠商
        public ActionResult Edit()
        {
            return View();
        }
        #endregion
    }
}