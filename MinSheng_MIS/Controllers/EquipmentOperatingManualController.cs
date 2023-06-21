using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Data.Entity.Migrations;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentOperatingManualController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
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
        [HttpPost]
        public ActionResult Create(EquipmentOperatingManualViewModel eom)
        {
            JObject jo = new JObject();
            #region 先檢查是否有同系統&子系統&設備名稱&廠牌&型號 之操作手冊存在
            var isexist = db.EquipmentOperatingManual.Where(x => x.System == eom.System && x.SubSystem == eom.SubSystem && x.EName == eom.EName && x.Brand == eom.Brand && x.Model == eom.Model);
            if(isexist.Count() > 0)
            {
                return Content("此操作手冊已存在", "application/json");
            }
            #endregion
            #region 新增設備操作手冊
            string Folder = Server.MapPath("~/Files/EquipmentOperatingManual");
            if (!Directory.Exists(Folder))
            {
                System.IO.Directory.CreateDirectory(Folder);
            }
            var lastEOMSN = db.EquipmentOperatingManual.OrderByDescending(x => x.EOMSN).FirstOrDefault();
            var num = 1;
            if(lastEOMSN != null)
            {
                num = Convert.ToInt32(lastEOMSN.EOMSN) + 1;
            }
            var newEOMSN = num.ToString().PadLeft(6, '0');
            string FolderPath = Server.MapPath("~/Files/EquipmentOperatingManual");
            string Filename = newEOMSN + Path.GetExtension(eom.ManualFile.FileName);
            System.IO.Directory.CreateDirectory(FolderPath);
            string filefullpath = Path.Combine(FolderPath, Filename);
            eom.ManualFile.SaveAs(filefullpath);

            var eomitem = new EquipmentOperatingManual();
            eomitem.EOMSN = newEOMSN;
            eomitem.System = eom.System;
            eomitem.SubSystem = eom.SubSystem;
            eomitem.EName = eom.EName;
            eomitem.Brand = eom.Brand;
            eomitem.Model = eom.Model;
            eomitem.FilePath = "/" + Filename;

            db.EquipmentOperatingManual.AddOrUpdate(eomitem);
            db.SaveChanges();
            #endregion
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
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