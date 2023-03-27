using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentInfo_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: EquipmentInfo_Management
        #region 資產管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 詳情
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 停用
        public ActionResult Disable()
        {
            return View();
        }
        #endregion

        #region 新增設備
        public ActionResult CreateEquipment()
        {
            return View();
        }
        #endregion

        #region 設備屬性
        [HttpGet]
        public ActionResult ReadBody(string id)
        {
            var equipmentInfo = db.EquipmentInfo.Find(id);
            //設備狀態 編碼對照顯示文字
            var dic = Surface.EState();
            equipmentInfo.EState = dic[equipmentInfo.EState];

            JsonSerializerSettings settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            string result = JsonConvert.SerializeObject(equipmentInfo, settings);
            return Content(result, "application/json");
        }
        #endregion
    }
}