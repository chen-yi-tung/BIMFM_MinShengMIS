using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class StoresRequisition_ManagementController : Controller
    {
        // GET: StoresRequisition_Management
        #region 領用申請管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 領用申請
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 領用申請詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯領用申請
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 審核領用申請
        public ActionResult Audit()
        {
            return View();
        }
        #endregion
    }
}