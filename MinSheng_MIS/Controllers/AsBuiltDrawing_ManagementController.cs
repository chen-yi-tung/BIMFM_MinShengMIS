using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class AsBuiltDrawing_ManagementController : Controller
    {
        // GET: AsBuiltDrawing_Management
        #region 竣工圖管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增竣工圖
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 檢視竣工圖
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯竣工圖
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除竣工圖
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}