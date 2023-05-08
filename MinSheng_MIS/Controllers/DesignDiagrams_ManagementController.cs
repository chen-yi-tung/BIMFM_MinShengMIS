using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class DesignDiagrams_ManagementController : Controller
    {
        // GET: AsBuiltDrawing_Management
        #region 設計圖管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增設計圖
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 檢視設計圖
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯設計圖
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除設計圖
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}