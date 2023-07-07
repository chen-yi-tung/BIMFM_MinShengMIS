using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
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
    public class AsBuiltDrawing_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: AsBuiltDrawing_Management
        #region 竣工圖管理
        public ActionResult Management()
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
            #region 編訂ADSN
            var newestADSN = db.AsBuiltDrawing.Where(x => x.UploadDate == DateTime.Today).OrderByDescending(x => x.ADSN).FirstOrDefault();
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

            #region 存竣工圖至指定路徑
            string Folder = Server.MapPath("~/Files/AsBuiltDrawing");
            if (!Directory.Exists(Folder))
            {
                System.IO.Directory.CreateDirectory(Folder);
            }
            string FolderPath = Server.MapPath("~/Files/AsBuiltDrawing");
            string Filename = ADSN + Path.GetExtension(info.ImgPath.FileName);
            System.IO.Directory.CreateDirectory(FolderPath);
            string filefullpath = Path.Combine(FolderPath, Filename);
            info.ImgPath.SaveAs(filefullpath);
            #endregion

            #region 存竣工圖資料進資料庫
            var adddrawing = new AsBuiltDrawingService();
            adddrawing.AddAsBuiltDrawing(info, ADSN, Filename);
            #endregion

            JObject jo = new JObject();
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion

        #region 檢視竣工圖
        public ActionResult Read()
        {
            return View();
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
            JObject jo = new JObject();

            string Filename = "";
            #region 存竣工圖
            if (drawing.ImgPath != null)
            {
                string file = db.AsBuiltDrawing.Find(drawing.ADSN).ImgPath.ToString();
                string fillfullpath = Server.MapPath($"~/Files/AsBuiltDrawing{file}");
                if (System.IO.File.Exists(fillfullpath))
                {
                    System.IO.File.Delete(fillfullpath);
                }
                string Folder = Server.MapPath("~/Files/AsBuiltDrawing");
                if (!Directory.Exists(Folder))
                {
                    System.IO.Directory.CreateDirectory(Folder);
                }
                string FolderPath = Server.MapPath("~/Files/AsBuiltDrawing");
                Filename = drawing.ADSN + Path.GetExtension(drawing.ImgPath.FileName);
                System.IO.Directory.CreateDirectory(FolderPath);
                string filefullpath = Path.Combine(FolderPath, Filename);
                drawing.ImgPath.SaveAs(filefullpath);
            }

            #endregion
            #region 編輯設備操作手冊至資料庫
            var adddrawing = new AsBuiltDrawingService();
            adddrawing.EditAsBuiltDrawing(drawing, Filename);
            #endregion
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
#endregion


        #region 刪除竣工圖
        public ActionResult Delete()
        {
            return View();
        }
        #endregion

        #region 指定竣工圖資訊
        [HttpGet]
        public ActionResult Readbody(string id)
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
            jo["ImgPath"] = item.ImgPath;
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);

            return Content(result, "application/json");
        }
        #endregion
    }
}