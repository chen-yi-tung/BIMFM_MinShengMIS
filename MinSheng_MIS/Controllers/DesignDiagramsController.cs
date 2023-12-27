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
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;

namespace MinSheng_MIS.Controllers
{
    public class DesignDiagramsController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 設計圖說管理
        public ActionResult Management()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DesignDiagrams_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_DesignDiagrams(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增設計圖說
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateDesignDiagrams(DesignDiagramsViewModel ddvm)
        {
            JObject jo = new JObject();
            DateTime today = DateTime.Now.Date;

            #region 檢查是否有同名稱&圖說類型之設計圖說存在
            var isexist = db.DesignDiagrams.Where(x => x.ImgName == ddvm.ImgName && x.ImgType == ddvm.ImgType);
            if (isexist.Count() > 0)
            {
                return Content("此設計圖說已存在!", "application/json");
            }
            #endregion

            #region 存設計圖說
            string Folder = Server.MapPath("~/Files/DesignDiagrams");
            if (!Directory.Exists(Folder))
            {
                System.IO.Directory.CreateDirectory(Folder);
            }

            var lastDDSN = db.DesignDiagrams.Where(x => x.UploadDate == today).OrderByDescending(x => x.DDSN).FirstOrDefault();
            var num = 1;
            if (lastDDSN != null)
            {
                num = Convert.ToInt32(lastDDSN.DDSN) % 100 + 1;
            }
            var newDDSN = today.ToString("yyMMdd") + num.ToString().PadLeft(2, '0');
            string FolderPath = Server.MapPath("~/Files/DesignDiagrams");
            string Filename = newDDSN + Path.GetExtension(ddvm.DesignDiagrams.FileName);
            System.IO.Directory.CreateDirectory(FolderPath);
            string filefullpath = Path.Combine(FolderPath, Filename);
            ddvm.DesignDiagrams.SaveAs(filefullpath);
            #endregion

            #region 新增設計圖說至資料庫
            var addDD = new DesignDiagramsService();
            addDD.AddDesignDiagrams(ddvm, newDDSN, Filename);
            #endregion

            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion

        #region  設計圖說詳情->檢視
        public ActionResult Read()
        {
            return View();
        }
        #endregion

        #region 編輯設計圖說
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult Readbody(string id)
        {
            JObject jo = new JObject();
            var item = db.DesignDiagrams.Find(id);
            jo["DDSN"] = item.DDSN;
            jo["ImgName"] = item.ImgName;
            jo["ImgType"] = item.ImgType;
            jo["DesignDiagrams"] = "/Files/DesignDiagrams" + item.ImgPath;
            jo["UploadDate"] = item.UploadDate;
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);

            return Content(result, "application/json");
        }
        [HttpGet]
        public ActionResult ReadbodyForDelete(string id)
        {
            JObject jo = new JObject();
            var item = db.DesignDiagrams.Find(id);
            jo["DDSN"] = item.DDSN;
            jo["ImgName"] = item.ImgName;
            var dic = Surface.ImgType();
            jo["ImgType"] = dic[item.ImgType];
            jo["DesignDiagrams"] = "/Files/DesignDiagrams" + item.ImgPath;
            jo["UploadDate"] = item.UploadDate;
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);

            return Content(result, "application/json");
        }
        [HttpPost]
        public ActionResult EditDesignDiagrams(DesignDiagramsViewModel ddvm)
        {
            JObject jo = new JObject();
            #region 檢查是否有同名稱&圖說類型之設計圖說存在
            var isexist = db.DesignDiagrams.Where(x => x.ImgName == ddvm.ImgName && x.ImgType == ddvm.ImgType && x.DDSN != ddvm.DDSN);
            if (isexist.Count() > 0)
            {
                return Content("此設計圖說已存在!", "application/json");
            }
            #endregion
            string Filename = "";
            #region 存設計圖說
            if (ddvm.DesignDiagrams != null)
            {
                string file = db.DesignDiagrams.Find(ddvm.DDSN).ImgPath.ToString();
                string fileFullPath = Server.MapPath($"~/Files/DesignDiagrams{file}");
                if (System.IO.File.Exists(fileFullPath))
                {
                    System.IO.File.Delete(fileFullPath);
                }
                string Folder = Server.MapPath("~/Files/DesignDiagrams");
                if (!Directory.Exists(Folder))
                {
                    System.IO.Directory.CreateDirectory(Folder);
                }
                string FolderPath = Server.MapPath("~/Files/DesignDiagrams");
                Filename = ddvm.DDSN + Path.GetExtension(ddvm.DesignDiagrams.FileName);
                System.IO.Directory.CreateDirectory(FolderPath);
                string filefullpath = Path.Combine(FolderPath, Filename);
                ddvm.DesignDiagrams.SaveAs(filefullpath);
            }

            #endregion
            #region 編輯設備操作手冊至資料庫
            var addDD = new DesignDiagramsService();
            addDD.EditDesignDiagrams(ddvm, ddvm.DDSN, Filename);
            #endregion
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion

        #region 刪除設計圖說
        public ActionResult Delete(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpPost]
        public ActionResult DeleteDesignDiagrams(string id)
        {
            JObject jo = new JObject();

            #region 刪除設備操作手冊
            var dd = db.DesignDiagrams.Find(id);
            string filefullpath = Server.MapPath($"~/Files/DesignDiagrams{dd.ImgPath}");
            if (System.IO.File.Exists(filefullpath))
            {
                System.IO.File.Delete(filefullpath);
            }

            #endregion
            #region 刪除設備操作手冊至資料庫
            db.DesignDiagrams.Remove(dd);
            db.SaveChanges();
            #endregion
            jo.Add("Succeed", true);
            string result = JsonConvert.SerializeObject(jo);
            return Content(result, "application/json");
        }
        #endregion
    }
}