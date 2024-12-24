using MinSheng_MIS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentOperatingManualController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 操作手冊
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 設備操作手冊
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 編輯 設備操作手冊
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除 設備操作手冊
        public ActionResult Delete(string id)
        {
            ViewBag.id = id;
            return View();
        }
        #endregion

        #region 設備操作手冊資料
        public ActionResult ReadBody(string id)
        {
            JObject jo = new JObject();
            var data = db.EquipmentOperatingManual.Find(id);


            return Content("53","application/json");
        }
        #endregion
    }
}