using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static MinSheng_MIS.Models.ViewModels.PathSampleViewModel;

namespace MinSheng_MIS.Controllers
{
    public class ManufacturerInfo_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        DatagridService ds = new DatagridService();
        // GET: ManufacturerInfo_Management
        #region 廠商管理
        public ActionResult Management()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ManufacturerInfo_Management(FormCollection form)
        {
            var service = new DatagridService();
            var a = ds.GetJsonForGrid_ManufacturerInfo_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增廠商
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 廠商詳情
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult ReadBody(string id)
        {
            var MFR = db.ManufacturerInfo.Find(id);
            //var MFR = db.ManufacturerInfo.Where(x => x.MFRSN == id).FirstOrDefault();//var MFR = db.ManufacturerInfo.Find(id)
            string result = JsonConvert.SerializeObject(MFR);
            return Content(result, "application/json");
        }
        #endregion

        #region 編輯廠商
        public ActionResult Edit()
        {
            return View();
        }
        #endregion
    }
}