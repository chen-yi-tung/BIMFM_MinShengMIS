using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class Stock_ManagementController : Controller
    {
        #region 庫存管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 庫存品項
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 新增 入庫填報
        public ActionResult CreateInbound()
        {
            return View();
        }
        #endregion

        #region 新增 出庫填報
        public ActionResult CreateOutbound()
        {
            return View();
        }
        #endregion


        #region 編輯 庫存品項
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 庫存品項 詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion

        #region 庫存品項 刪除
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}