using MinSheng_MIS.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Models;
using System.Web.Services.Description;

namespace MinSheng_MIS.Controllers
{
    public class DatagridController : Controller
    {
        private readonly DatagridService _service = new DatagridService();

        //--巡檢管理--
        #region InspectionPlan_Management 巡檢計畫管理
        //[HttpPost]
        //public ActionResult InspectionPlan_Management(FormCollection form)
        //{
        //    
        //    var a = _service.GetJsonForGrid_InspectionPlan(form);
        //    string result = JsonConvert.SerializeObject(a);
        //    return Content(result, "application/json");
        //}
        #endregion

        #region SamplePath_Management 巡檢路線模板管理
        [HttpPost]
        public ActionResult SamplePath_Management(FormCollection form)
        {
            
            var a = _service.GetJsonForGrid_SamplePath(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region SampleSchedule_Management 每日巡檢時程模板管理
        [HttpPost]
        public ActionResult SampleSchedule_Management(FormCollection form)
        {

            var a = _service.GetJsonForGrid_DailyInspectionSample(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region InspectationPlan_Record_EquipMaintain 巡檢紀錄_設備保養紀錄
        //[HttpPost]
        //public ActionResult InspectationPlan_Record_EquipMaintain(FormCollection form)
        //{
        //    
        //    string result = _service.GetJsonForGrid_InspectationPlan_Record_EquipMaintain(form);
        //    return Content(result, "application/json");
        //}
        #endregion

        #region InspectationPlan_Record_EquipRepair 巡檢紀錄_設備維修紀錄
        //[HttpPost]
        //public ActionResult InspectationPlan_Record_EquipRepair(FormCollection form)
        //{
        //    
        //    string result = _service.GetJsonForGrid_InspectationPlan_Record_EquipRepair(form);
        //    return Content(result, "application/json");
        //}
        #endregion


        //--定期保養管理--
        #region MaintainForm_Management 定期保養單管理
        //[HttpPost]
        //public ActionResult MaintainForm_Management(FormCollection form)
        //{
        //    
        //    var a = _service.GetJsonForGrid_MaintainForm(form);
        //    string result = JsonConvert.SerializeObject(a);
        //    return Content(result, "application/json");
        //}
        #endregion

        #region MaintainItem_Management 保養項目管理
        //[HttpPost]
        //public ActionResult MaintainItem_Management(FormCollection form)
        //{
        //    
        //    var a = _service.GetJsonForGrid_MaintainItem(form);
        //    string result = JsonConvert.SerializeObject(a);
        //    return Content(result, "application/json");
        //}
        #endregion

        #region EquipmentMaintainPeriod_Management 設備保養週期管理
        //[HttpPost]
        //public ActionResult EquipmentMaintainPeriod_Management(FormCollection form)
        //{
        //    JObject jo = new JObject();
        //    
        //    var a = _service.GetJsonForGrid_EquipmentMaintainPeriod_Management(form);
        //    string result = JsonConvert.SerializeObject(a);
        //    return Content(result, "application/json");
        //}
        #endregion

        #region MaintainRecord_Management 巡檢保養紀錄管理
        //[HttpPost]
        //public ActionResult MaintainRecord_Management(FormCollection form)
        //{
        //    
        //    var a = _service.GetJsonForGrid_MaintainRecord_Management(form);
        //    string result = JsonConvert.SerializeObject(a);
        //    return Content(result, "application/json");
        //}
        #endregion


        //--報修管理--
        #region Report_Management 報修管理
        //[HttpPost]
        //public ActionResult Report_Management(FormCollection form)
        //{
        //    
        //    var a = _service.GetJsonForGrid_Report_Management(form);
        //    string result = JsonConvert.SerializeObject(a);
        //    return Content(result, "application/json");
        //}
        #endregion

        #region RepairRecord_Management 巡檢維修紀錄管理
        //[HttpPost]
        //public ActionResult RepairRecord_Management(FormCollection form)
        //{
        //    
        //    var a = _service.GetJsonForGrid_RepairRecord_Management(form);
        //    string result = JsonConvert.SerializeObject(a);
        //    return Content(result, "application/json");
        //}
        #endregion


        //--設備管理--
        #region EquipmentInfo_Management 資產管理
        [HttpPost]
        public ActionResult EquipmentInfo_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_EquipmentInfo(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region OneDeviceOneCard_Management 一機一卡模板管理
        [HttpPost]
        public ActionResult OneDeviceOneCard_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_OneDeviceOneCard(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region AsBuiltDrawing_Management 竣工圖說管理
        [HttpPost]
        public ActionResult AsBuiltDrawing_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_AsBuiltDrawing(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region DesignDiagrams 設計圖說管理
        [HttpPost]
        public ActionResult DesignDiagrams_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_DesignDiagrams(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region EquipmentOperatingManual 設備操作手冊
        [HttpPost]
        public ActionResult EquipmentOperatingManual(FormCollection form)
        {

            var a = _service.GetJsonForGrid_EquipmentOperatingManual(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion


        //--庫存管理--
        #region Stock_Management 庫存管理
        [HttpPost]
        public ActionResult Stock_Management(FormCollection form)
        {

            var a = _service.GetJsonForGrid_Stock_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion


        //--實驗室管理--
        #region TestingAndAnalysisWorkflow 採驗分析流程建立
        [HttpPost]
        public ActionResult TestingAndAnalysisWorkflow(FormCollection form)
        {
            var a = _service.GetJsonForGrid_TestingAndAnalysisWorkflow(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region LaboratoryLabel_Management 實驗室標籤管理
        [HttpPost]
        public ActionResult LaboratoryLabel_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_LaboratoryLabel_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region LaboratoryMaintenance_Management 實驗室維護管理
        [HttpPost]
        public ActionResult LaboratoryMaintenance_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_LaboratoryMaintenance_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region ExperimentData_Management 實驗數據管理
        [HttpPost]
        public ActionResult ExperimentData_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_ExperimentData_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion


        //--警示訊息管理--
        #region WarningMessage_Management 警示訊息管理
        //[HttpPost]
        //public ActionResult WarningMessage_Management(FormCollection form)
        //{
        //    
        //    var a = _service.GetJsonForGrid_WarningMessage_Management(form);
        //    string result = JsonConvert.SerializeObject(a);
        //    return Content(result, "application/json");
        //}
        #endregion


        //--文件管理--
        #region MonthlyReport_Management 月報管理
        [HttpPost]
        public ActionResult MonthlyReport_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_MonthlyReport_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region MeetingMinutes_Management 會議記錄管理
        [HttpPost]
        public ActionResult MeetingMinutes_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_MeetingMinutes_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion


        //--系統管理--
        #region Account_Management 帳號管理
        [HttpPost]
        public ActionResult Account_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_Account_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion

        #region ManufacturerInfo_Management 廠商管理
        [HttpPost]
        public ActionResult ManufacturerInfo_Management(FormCollection form)
        {
            var a = _service.GetJsonForGrid_ManufacturerInfo_Management(form);
            string result = JsonConvert.SerializeObject(a);
            return Content(result, "application/json");
        }
        #endregion
    }
}