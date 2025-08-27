using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Services.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
        [HttpPost]
        public ActionResult Create(EquipmentOperatingManualViewModel eom)
        {
            try
            {
                JObject jo = new JObject();
                #region 先檢查是否有同設備名稱&廠牌&型號 之操作手冊存在
                var isexist = db.EquipmentOperatingManual.Where(x => x.EName == eom.EName && x.Brand == eom.Brand && x.Model == eom.Model);
                if (isexist.Count() > 0)
                {
                    return Content("此操作手冊已存在!", "application/json");
                }
                #endregion
                #region 存設備操作手冊
                var (isValid, erroeMessage) = ComFunc.IsConformedForImageAndPdf(eom.ManualFile);
                if (!isValid)
                {
                    return Content(erroeMessage, "application/json");
                }
                else
                {
                    string Folder = Server.MapPath("~/Files/EquipmentOperatingManual");
                    System.IO.Directory.CreateDirectory(Folder);
                    var lastEOMSN = db.EquipmentOperatingManual.OrderByDescending(x => x.EOMSN).FirstOrDefault();
                    var num = 1;
                    if (lastEOMSN != null)
                    {
                        num = Convert.ToInt32(lastEOMSN.EOMSN) + 1;
                    }
                    var newEOMSN = num.ToString().PadLeft(6, '0');
                    string Filename = newEOMSN + Path.GetExtension(eom.ManualFile.FileName);
                    string filefullpath = Path.Combine(Folder, Filename);
                    eom.ManualFile.SaveAs(filefullpath);
                    #endregion
                    #region 新增設備操作手冊至資料庫
                    var addeom = new EquipmentOperatingManualService();
                    addeom.AddEquipmentOperatingManual(eom, newEOMSN, Filename);
                    #endregion
                    jo.Add("Succeed", true);
                    string result = JsonConvert.SerializeObject(jo);
                    return Content(result, "application/json");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
                return Content(string.Join(",", ex.Message));
            }
        }
        #endregion

        #region 編輯 設備操作手冊
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult Readbody(string id)
        {
            try
            {
                JObject jo = new JObject();
                var item = db.EquipmentOperatingManual.Find(id);
                jo["EOMSN"] = item.EOMSN;
                jo["EName"] = item.EName;
                jo["Brand"] = item.Brand;
                jo["Model"] = item.Model;
                jo["ManualFile"] = !string.IsNullOrEmpty(item.FilePath) ? "/Files/EquipmentOperatingManual" + item.FilePath : null;
                jo.Add("Succeed", true);
                string result = JsonConvert.SerializeObject(jo);

                return Content(result, "application/json");
            }
            catch(Exception ex)
            {
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
                return Content(string.Join(",", ex.Message));
            }
        }
        [HttpPost]
        public ActionResult Edit(EquipmentOperatingManualViewModel eom)
        {
            try
            {
                JObject jo = new JObject();
                #region 先檢查是否有同設備名稱&廠牌&型號 之操作手冊存在
                var isexist = db.EquipmentOperatingManual.Where(x => x.EName == eom.EName && x.Brand == eom.Brand && x.Model == eom.Model && x.EOMSN != eom.EOMSN);
                if (isexist.Count() > 0)
                {
                    return Content("此操作手冊已存在!", "application/json");
                }
                #endregion
                string Filename = "";
                #region 存設備操作手冊
                if (eom.ManualFile != null)
                {
                    var (isValid, erroeMessage) = ComFunc.IsConformedForImageAndPdf(eom.ManualFile);
                    if (!isValid)
                    {
                        return Content(erroeMessage, "application/json");
                    }
                    else
                    {
                        string file = db.EquipmentOperatingManual.Find(eom.EOMSN).FilePath.ToString();
                        string fillfullpath = Server.MapPath($"~/Files/EquipmentOperatingManual{file}");
                        if (System.IO.File.Exists(fillfullpath))
                        {
                            System.IO.File.Delete(fillfullpath);
                        }
                        string Folder = Server.MapPath("~/Files/EquipmentOperatingManual");
                        System.IO.Directory.CreateDirectory(Folder);
                        Filename = eom.EOMSN + Path.GetExtension(eom.ManualFile.FileName);
                        string filefullpath = Path.Combine(Folder, Filename);
                        eom.ManualFile.SaveAs(filefullpath);
                    }
                }

                #endregion
                #region 編輯設備操作手冊至資料庫
                var addeom = new EquipmentOperatingManualService();
                addeom.EditEquipmentOperatingManual(eom, eom.EOMSN, Filename);
                #endregion
                jo.Add("Succeed", true);
                string result = JsonConvert.SerializeObject(jo);
                return Content(result, "application/json");
            }
            catch(Exception ex)
            {
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
                return Content(string.Join(",", ex.Message));
            }
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
            try
            {
                JObject jo = new JObject();
                var data = db.EquipmentOperatingManual.Find(id);

                return Content("53", "application/json");
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
                return Content(string.Join(",", ex.Message));
            }
        }
        [HttpPost]
        public ActionResult DeleteEOM(string id)
        {
            try
            {
                JObject jo = new JObject();

                #region 刪除設備操作手冊
                var eom = db.EquipmentOperatingManual.Find(id);
                string fillfullpath = Server.MapPath($"~/Files/EquipmentOperatingManual{eom.FilePath}");
                if (System.IO.File.Exists(fillfullpath))
                {
                    System.IO.File.Delete(fillfullpath);
                }

                #endregion
                #region 刪除設備操作手冊至資料庫
                db.EquipmentOperatingManual.Remove(eom);
                db.SaveChanges();
                #endregion
                jo.Add("Succeed", true);
                string result = JsonConvert.SerializeObject(jo);
                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
                return Content(string.Join(",", ex.Message));
            }
        }
        #endregion
    }
}