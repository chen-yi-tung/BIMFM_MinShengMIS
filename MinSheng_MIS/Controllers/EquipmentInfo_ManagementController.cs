using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        [HttpPost]
        public ActionResult EquipmentInfo_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_EquipmentInfo(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
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
        [HttpPost]
        public ActionResult Create(EquipmentInfo_ManagementViewModel eim)
        {
            JObject jo = new JObject();

            #region 先檢查是否有同國有財產編碼 之設備存在
            var isexist = db.EquipmentInfo.Where(x => x.PropertyCode == eim.PropertyCode);
            if (isexist.Count() > 0)
            {
                return Content("此設備已存在!", "application/json");
            }
            #endregion

            var lastESN = db.EquipmentInfo.OrderByDescending(x => x.ESN).FirstOrDefault();
            var num = 1;
            if (lastESN != null)
            {
                num = Convert.ToInt32(lastESN.ESN) + 1;
            }
            var newESN = 'E' + num.ToString().PadLeft(5, '0');

            #region 存設備操作手冊
            string Filename = "";
            if (eim.ManualFile != null)
            {
                var lastEOMSN = db.EquipmentOperatingManual.OrderByDescending(x => x.EOMSN).FirstOrDefault();
                var EOMSNnum = 1;
                if (lastEOMSN != null)
                {
                    EOMSNnum = Convert.ToInt32(lastEOMSN.EOMSN) + 1;
                }
                var EOMSN = EOMSNnum.ToString().PadLeft(6, '0');

                #region 檢查是否需要新增或更新設備操作手冊
                var manualexist = db.EquipmentOperatingManual.Where(x => x.System == eim.System && x.SubSystem == eim.SubSystem && x.EName == eim.EName && x.Brand == eim.Brand && x.Model == eim.Model);
                if (manualexist.Count() > 0)
                {
                    string file = manualexist.FirstOrDefault().FilePath.ToString();

                    string fillfullpath = Server.MapPath($"~/Files/EquipmentOperatingManual{file}");
                    if (System.IO.File.Exists(fillfullpath))
                    {
                        System.IO.File.Delete(fillfullpath);
                    }
                    EOMSN = manualexist.FirstOrDefault().EOMSN;
                }
                string Folder = Server.MapPath("~/Files/EquipmentOperatingManual");
                if (!Directory.Exists(Folder))
                {
                    System.IO.Directory.CreateDirectory(Folder);
                }
                string FolderPath = Server.MapPath("~/Files/EquipmentOperatingManual");
                Filename = EOMSN + Path.GetExtension(eim.ManualFile.FileName);
                System.IO.Directory.CreateDirectory(FolderPath);
                string filefullpath = Path.Combine(FolderPath, Filename);
                eim.ManualFile.SaveAs(filefullpath);
                #endregion

                #region 新增設備操作手冊至資料庫
                var eom = new EquipmentOperatingManualViewModel();
                eom.System = eim.System;
                eom.SubSystem = eim.SubSystem;
                eom.EName = eim.EName;
                eom.Brand = eim.Brand;
                eom.Model = eim.Model;
                var addeom = new EquipmentOperatingManualService();
                if (manualexist.Count() > 0)
                {
                    addeom.AddEquipmentOperatingManual(eom, EOMSN, Filename);
                }
                else
                {
                    addeom.EditEquipmentOperatingManual(eom, EOMSN, Filename);
                }
                #endregion

            }
            #endregion

            #region 新增設備至資料庫
            var addeim = new EquipmentInfo_ManagementService();
            addeim.AddEquipmentInfo(eim, newESN);
            #endregion
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
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