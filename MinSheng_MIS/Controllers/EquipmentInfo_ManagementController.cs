using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentInfo_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: EquipmentInfo_Management
        public ActionResult Index()
        {
            return View();
        }

        #region 設備屬性
        [HttpGet]
        public ActionResult ReadBody(string id)
        {
            var EquipmentInfo = db.EquipmentInfo.Find(id);
            var dic = Surface.EState();
            EquipmentInfo.EState = dic[EquipmentInfo.EState];
            string result = JsonConvert.SerializeObject(EquipmentInfo);
            return Content(result, "application/json");
        }
        #endregion
    }
}