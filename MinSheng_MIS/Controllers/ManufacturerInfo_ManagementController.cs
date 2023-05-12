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
using System.Data.Entity.Migrations;
using Newtonsoft.Json.Linq;
using System.Web.Services.Description;

namespace MinSheng_MIS.Controllers
{
    public class ManufacturerInfo_ManagementController : Controller
    {
        ManufacturerInfo_ViewModel ManufacturerInfoVM = new ManufacturerInfo_ViewModel();

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
            var a = service.GetJsonForGrid_ManufacturerInfo_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region 新增廠商
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(FormCollection form) 
        {

            return Content("");
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
            int resultCode = 200;
            string result = ManufacturerInfoVM.Manufac_Read_GetData(id, ref resultCode);
            Response.StatusCode = resultCode;
            return Content(result, "application/json");
        }
        #endregion

        #region 編輯廠商
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpPost]
        public ActionResult EditMFR(ManufacturerInfo MFR)
        {
            int resultCode = 200;

            #region 基本檢查
            if (string.IsNullOrEmpty(MFR.MFRName))
            {
                resultCode = 400;
                JsonResponseViewModel Jresult = new JsonResponseViewModel()
                {
                    ResponseCode = 200,
                    ResponseMessage = "供應商名稱為必填項目!"
                };
                Response.StatusCode = resultCode;
                return Content(JsonConvert.SerializeObject(Jresult), "application/json");
            }
            #endregion
            
            string result = ManufacturerInfoVM.Manufac_Edit_Update(MFR, ref resultCode);
            Response.StatusCode = resultCode;
            return Content(result, "application/json");
        }
        #endregion
    }
}