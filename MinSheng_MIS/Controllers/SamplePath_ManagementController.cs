using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace MinSheng_MIS.Controllers
{
    public class SamplePath_ManagementController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly RFIDService _rfidService;
        private readonly SamplePath_ManagementService _samplePathService;

        public SamplePath_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _rfidService = new RFIDService(_db);
            _samplePathService = new SamplePath_ManagementService(_db);
        }

        #region 巡檢路線模板管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 巡檢路線模板
        public ActionResult Create()
        {
            return View();
        }
        #region 欲新增的設備Grid
        //[HttpPost]
        //public ActionResult AddEquipmentGrid(FormCollection form)
        //{
        //    //todo
        //    var a = _samplepathService.GetJsonForGrid_DailyInspectionSample(form);
        //    string result = JsonConvert.SerializeObject(a);
        //    return Content(result, "application/json");
        //}
        #endregion
        #endregion

        #region 編輯 巡檢路線模板
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 巡檢路線模板 詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion

        #region 巡檢路線模板 刪除
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}