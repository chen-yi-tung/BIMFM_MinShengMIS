using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentOperatingManualController : Controller
    {
        // GET: EquipmentOperatingManual
        #region 設備操作手冊管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增設備操作手冊
        public ActionResult Create()
        {
            return View();
        }
        #endregion
        #region  設備操作手冊詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion
        #region 編輯設備操作手冊
        public ActionResult Edit()
        {
            return View();
        }
        #endregion
        #region 刪除設備操作手冊
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}