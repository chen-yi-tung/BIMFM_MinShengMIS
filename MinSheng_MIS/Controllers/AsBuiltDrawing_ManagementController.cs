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
        public ActionResult Create()
        {
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
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 刪除竣工圖
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}