using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class Account_ManagementController : Controller
    {
        // GET: Account_Management
        #region 帳號管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增帳號
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 編輯帳號
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除帳號
        public ActionResult Delete()
        {
            return View();
        }
        #endregion

        #region 變更密碼
        public ActionResult ChangePassword()
        {
            return View();
        }
        #endregion
    }
}