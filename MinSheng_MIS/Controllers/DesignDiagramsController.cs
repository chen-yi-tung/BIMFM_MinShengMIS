using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class DesignDiagramsController : Controller
    {
        // GET: DesignDiagrams
        #region 設計圖說管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 新增設計圖說
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 編輯設計圖說
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除設計圖說
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}