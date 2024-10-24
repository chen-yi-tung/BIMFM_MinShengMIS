using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentInfo_ManagementController : Controller
    {
        #region 資產管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 設備
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 編輯 設備
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 設備 詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion

        #region 刪除 設備
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}