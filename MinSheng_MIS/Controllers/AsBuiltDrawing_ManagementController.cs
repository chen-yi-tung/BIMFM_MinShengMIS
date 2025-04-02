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
using System.Web.Http.Results;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class AsBuiltDrawing_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: AsBuiltDrawing_Management
        #region 竣工圖管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增竣工圖
        public ActionResult Create(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpPost]
        public ActionResult Create(AsBuiltDrawingViewModel info)
        {
            try
            {
                #region 檢查是否有同樓層&同子系統別&同圖號&同圖名&同板本
                var isexist = db.AsBuiltDrawing.Where(x => x.FSN == info.FSN && x.DSubSystemID == info.DSubSystemID && x.ImgNum == info.ImgNum && x.ImgName == info.ImgName && x.ImgVersion == info.ImgVersion);
                if (isexist.Count() > 0)
                {
                    return Content("<br>此竣工圖說已存在!", "application/json");
                }
                #endregion

                JObject jo = new JObject();

                #region 存竣工圖至指定路徑
                #region 檢查檔案格式
                var (isValid, erroeMessage) = ComFunc.IsConformedForImageAndPdf(info.ImgPath);
                if (!isValid)
                {
                    return Content(erroeMessage, "application/json");
                }
                else
                {
                    #region 編訂ADSN
                    var todayPrefix = DateTime.Today.ToString("yyMMdd");
                    var newestADSN = db.AsBuiltDrawing.Where(x => x.ADSN.StartsWith(todayPrefix)).OrderByDescending(x => x.ADSN).FirstOrDefault();
                    var ADSN = "";
                    if (newestADSN == null)
                    {
                        ADSN = DateTime.Today.Date.ToString("yyMMdd") + "01";
                    }
                    else
                    {
                        var intADSN = Convert.ToInt32(newestADSN.ADSN.ToString());
                        ADSN = (intADSN + 1).ToString();
                    }
                    #endregion
                    string Folder = Server.MapPath("~/Files/AsBuiltDrawing");
                    System.IO.Directory.CreateDirectory(Folder);
                    string Filename = ADSN + Path.GetExtension(info.ImgPath.FileName);
                    string filefullpath = Path.Combine(Folder, Filename);
                    info.ImgPath.SaveAs(filefullpath);
                    #endregion

                    #region 存竣工圖資料進資料庫
                    var adddrawing = new AsBuiltDrawingService();
                    adddrawing.AddAsBuiltDrawing(info, ADSN, Filename);
                    #endregion
                    jo.Add("Succeed", true);
                    string result = JsonConvert.SerializeObject(jo);

                    return Content(result, "application/json");
                }
                #endregion
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
                return Content(string.Join(",", ex.Message));
            }
            
        }
        #endregion

        #region 編輯竣工圖
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpPost]
        public ActionResult Edit(AsBuiltDrawingViewModel drawing)
        {
            try
            {
                #region 檢查是否有同樓層&同子系統別&同圖號&同圖名&同板本
                var isexist = db.AsBuiltDrawing.Where(x => x.FSN == drawing.FSN && x.DSubSystemID == drawing.DSubSystemID && x.ImgNum == drawing.ImgNum && x.ImgName == drawing.ImgName && x.ImgVersion == drawing.ImgVersion && x.ADSN != drawing.ADSN);
                if (isexist.Count() > 0)
                {
                    return Content("<br>此竣工圖說已存在!", "application/json");
                }
                #endregion

                JObject jo = new JObject();
                string result = "";
                string Filename = "";
                #region 存竣工圖
                if (drawing.ImgPath != null)
                {
                    #region 檢查檔案格式
                    var (isValid, erroeMessage) = ComFunc.IsConformedForImageAndPdf(drawing.ImgPath);
                    if (!isValid)
                    {
                        return Content(erroeMessage, "application/json");
                    }
                    else
                    {
                        string file = db.AsBuiltDrawing.Find(drawing.ADSN).ImgPath.ToString();
                        string fillfullpath = Server.MapPath($"~/Files/AsBuiltDrawing{file}");
                        if (System.IO.File.Exists(fillfullpath))
                        {
                            System.IO.File.Delete(fillfullpath);
                        }
                        string Folder = Server.MapPath("~/Files/AsBuiltDrawing");
                        System.IO.Directory.CreateDirectory(Folder);
                        Filename = drawing.ADSN + Path.GetExtension(drawing.ImgPath.FileName);
                        string filefullpath = Path.Combine(Folder, Filename);
                        drawing.ImgPath.SaveAs(filefullpath);
                    }
                    #endregion
                }
                #endregion
                #region 編輯設備操作手冊至資料庫
                var adddrawing = new AsBuiltDrawingService();
                adddrawing.EditAsBuiltDrawing(drawing, Filename);
                #endregion
                result = JsonConvert.SerializeObject(jo);
                return Content(result, "application/json");
            }
            catch(Exception ex)
            {
                LogHelper.WriteErrorLog(this, User.Identity.Name, ex.Message.ToString());
                return Content(string.Join(",", ex.Message));
            }
        }
        #endregion

        #region 刪除竣工圖
        public ActionResult Delete(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpPost]
        public ActionResult DeleteAsBuiltDrawing(string id)
        {
            try
            {
                JObject jo = new JObject();

                #region 刪除竣工圖
                var drawing = db.AsBuiltDrawing.Find(id);
                string fillfullpath = Server.MapPath($"~/Files/AsBuiltDrawing{drawing.ImgPath}");
                if (System.IO.File.Exists(fillfullpath))
                {
                    System.IO.File.Delete(fillfullpath);
                }

                #endregion
                #region 刪除設備操作手冊至資料庫
                db.AsBuiltDrawing.Remove(drawing);
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

        #region 指定竣工圖資訊
        [HttpGet]
        public ActionResult Readbody(string id)
        {
            try
            {
                JObject jo = new JObject();
                var item = db.AsBuiltDrawing.Find(id);
                var ASN = db.Floor_Info.Find(item.FSN).ASN.ToString();
                jo["ADSN"] = item.ADSN;
                jo["ASN"] = ASN;
                jo["FSN"] = item.FSN;
                var DSystemID = db.DrawingSubSystemManagement.Find(item.DSubSystemID).DSystemID;
                jo["DSystemID"] = DSystemID;
                jo["DSubSystemID"] = item.DSubSystemID;
                jo["ImgNum"] = item.ImgNum;
                jo["ImgName"] = item.ImgName;
                jo["ImgVersion"] = item.ImgVersion;
                jo["ImgPath"] = !string.IsNullOrEmpty(item.ImgPath) ? "/Files/AsBuiltDrawing" + item.ImgPath : null;
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

        #region 指定竣工圖資訊
        [HttpGet]
        public ActionResult ReadAsBuiltDrawingbody(string id)
        {
            try
            {
                JObject jo = new JObject();
                var item = db.AsBuiltDrawing.Find(id);
                var ASN = db.Floor_Info.Find(item.FSN).ASN;
                var Area = db.AreaInfo.Find(ASN).Area.ToString();
                jo["Area"] = Area;
                var Floor = db.Floor_Info.Find(item.FSN).FloorName.ToString();
                jo["Floor"] = Floor;
                var SystemInfo = db.DrawingSubSystemManagement.Find(item.DSubSystemID);
                var DSystem = db.DrawingSystemManagement.Find(SystemInfo.DSystemID).DSystem.ToString();
                jo["DSystem"] = DSystem;
                jo["DSubSystem"] = SystemInfo.DSubSystem;
                jo["ImgNum"] = item.ImgNum;
                jo["ImgName"] = item.ImgName;
                jo["ImgVersion"] = item.ImgVersion;
                jo["UploadDate"] = item.UploadDate.ToString("yyyy-MM-dd HH:mm:ss");
                jo["ImgPath"] = !string.IsNullOrEmpty(item.ImgPath) ? "/Files/AsBuiltDrawing" + item.ImgPath : null;
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
    }
}