using MinSheng_MIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace MinSheng_MIS.Attributes
{
    public class MinShengAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = httpContext.User as ClaimsPrincipal;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }
            else
            {
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                using (Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities())
                {
                    var controllerAction = httpContext.Request.RequestContext.RouteData.Values["controller"]?.ToString() + "/" + httpContext.Request.RequestContext.RouteData.Values["action"]?.ToString();
                    var Authority = db.AspNetUsers.Find(userId).Authority;
                    httpContext.Session["Authority"] = Authority;
                    switch (Authority)
                    {
                        case "1":
                            return true;
                        case "2":
                            if (OperatorDeniedPrivileges.Contains(controllerAction)) return false;
                            break;
                        case "3":
                            if (OperatorDeniedPrivileges.Contains(controllerAction) || GeneralDeniedPrivileges.Contains(controllerAction)) return false;
                            break;
                        default:
                            return false;
                    }
                }
            }
            return true;
        }

        //操作者"禁止"權限
        readonly private List<string> OperatorDeniedPrivileges = new List<string>
        {
            #region 工單管理
            #region 刪除
            "PlanManagement/Delete",
            "PlanManagement/DeleteInspectionPlan",
            #endregion
            #endregion

            #region 巡檢路線模板管理
            #region 刪除
            "SamplePath_Management/Delete",
            "SamplePath_Management/DeleteSamplePath",
            #endregion
            #endregion

            #region 每日巡檢時程模板管理
            #region 刪除
            "SampleSchedule_Management/Delete",
            "SampleSchedule_Management/DeleteSampleSchedule",
            #endregion
            #endregion

            #region 定期保養管理
            #region 審核
            "Maintain_Management/Review",
            "Maintain_Management/Audit",
            #endregion
            #endregion

            #region 報修管理
            #region 審核
            "Repair_Management/Review",
            "Repair_Management/Audit",
            #endregion
            #endregion
            
            #region 資產管理
            #region 停用
            "EquipmentInfo_Management/Delete",
            "EquipmentInfo_Management/DisableEquipment",
            #endregion
            #endregion
            
            #region 一機一卡模板管理
            #region 刪除
            "OneDeviceOneCard_Management/Delete",
            "OneDeviceOneCard_Management/DeleteDeviceCard",
            #endregion
            #endregion
            
            #region 竣工圖管理
            #region 刪除
            "AsBuiltDrawing_Management/Delete",
            "AsBuiltDrawing_Management/DeleteAsBuiltDrawing",
            #endregion
            #endregion
            
            #region 設計圖說管理
            #region 刪除
            "DesignDiagrams/Delete",
            "DesignDiagrams/DeleteDesignDiagrams",
            #endregion
            #endregion
            
            #region 設備操作手冊
            #region 刪除
            "EquipmentOperatingManual/Delete",
            "EquipmentOperatingManual/DeleteEOM",
            #endregion
            #endregion
            
            #region 庫存管理
            #region 刪除
            "Stock_Management/Delete",
            "Stock_Management/DeleteComputationalStock",
            #endregion
            #endregion
            
            #region 月報管理
            #region 刪除
            "MonthlyReport_Management/Delete",
            "MonthlyReport_Management/Delete_MonthlyReport",
            #endregion
            #endregion
            
            #region 會議記錄管理
            #region 刪除
            "MeetingMinutes_Management/Delete",
            "MeetingMinutes_Management/DeleteMeetingMinutes",
            #endregion
            #endregion

            #region 帳號管理
            #region 新增帳號
            "Account_Management/Create",
            "Account_Management/Create_Add",
            #endregion
            #region 編輯帳號
            "Account_Management/Edit",
            "Account_Management/Edit_LoadData",
            #endregion
            #region 刪除帳號
            "Account_Management/Delete",
            "Account_Management/Delete_LoadData",
            #endregion
            #endregion
        };

        //一般使用者"禁止"權限
        readonly private List<string> GeneralDeniedPrivileges = new List<string>
        {
            #region 工單管理
            #region 新增工單
            "PlanManagement/Create",
            "PlanManagement/CreateInspectionPlan",
            #endregion
            #region 編輯
            "PlanManagement/Edit",
            "PlanManagement/EditInspectionPlan",
            #endregion
            #region 下載
            "InspectionPlan_Management/GetInspectionExcel",
            #endregion
            #endregion

            #region 巡檢路線模板管理
            #region 新增模板
            "SamplePath_Management/Create",
            "SamplePath_Management/AddEquipmentRFIDsGrid",
            "SamplePath_Management/CreateSamplePath",
            #endregion
            #region 編輯
            "SamplePath_Management/Edit",
            "SamplePath_Management/EditSamplePath",
            #endregion
            #endregion

            #region 每日巡檢時程模板管理
            #region 新增模板
            "SampleSchedule_Management/Create",
            "SampleSchedule_Management/CreateSampleSchedule",
            #endregion
            #region 編輯
            "SampleSchedule_Management/Edit",
            "SampleSchedule_Management/EditSampleSchedule",
            #endregion
            #endregion

            #region 定期保養管理
            #region 派工/批量派工
            "Maintain_Management/Assignment",
            #endregion
            #endregion

            #region 報修管理            
            #region 新增報修
            "Repair_Management/Create",
            #endregion
            #region 派工/批量派工
            "Repair_Management/Assignment",
            #endregion
            #endregion
            
            #region 資產管理
            #region 新增設備
            "EquipmentInfo_Management/Create",
            "EquipmentInfo_Management/CreateEquipment",
            #endregion
            #region 編輯
            "EquipmentInfo_Management/Edit",
            "EquipmentInfo_Management/EditEquipment",
            #endregion
            #region 匯出
            "EquipmentInfo_Management/ExportToExcel",
            #endregion
            #endregion

            #region 一機一卡模板管理
            #region 新增模板
            "OneDeviceOneCard_Management/Create",
            "OneDeviceOneCard_Management/CreateDeviceCard",
            #endregion
            #region 編輯
            "OneDeviceOneCard_Management/Edit",
            "OneDeviceOneCard_Management/GetEquipmentsUsingTemplate",
            "OneDeviceOneCard_Management/EditDeviceCard",
            #endregion
            #endregion
            
            #region 竣工圖管理
            #region 新增竣工圖
            "AsBuiltDrawing_Management/Create",
            #endregion
            #region 編輯
            "AsBuiltDrawing_Management/Edit",
            #endregion
            #endregion
            
            #region 設計圖說管理
            #region 新增設計圖
            "DesignDiagrams/Create",
            "DesignDiagrams/CreateDesignDiagrams",
            #endregion
            #region 編輯
            "DesignDiagrams/Edit",
            "DesignDiagrams/EditDesignDiagrams",
            #endregion
            #endregion
            
            #region 設備操作手冊
            #region 新增
            "EquipmentOperatingManual/Create",
            #endregion
            #region 編輯
            "EquipmentOperatingManual/Edit",
            #endregion
            #endregion
            
            #region 庫存管理
            #region 新增品項
            "Stock_Management/Create",
            "Stock_Management/CreateComputationalStock",
            #endregion
            #region 新增入庫填報
            "Stock_Management/CreateInbound",
            "Stock_Management/CreateNormalComputationalStockIn",
            #endregion
            #region 新增出庫填報
            "Stock_Management/CreateOutbound",
            "Stock_Management/CreateNormalComputationalStockOut",
            #endregion
            #region 編輯
            "Stock_Management/Edit",
            "Stock_Management/EditComputationalStock",
            #endregion            
            #region 庫存管理匯出
            "Stock_Management/ExportToExcel",
            #endregion            
            #region 庫存管理詳情匯出
            "Stock_Management/DetailExportToExcel",
            #endregion
            #endregion
            
            #region 採驗分析流程建立
            #region 新增
            "TestingAndAnalysisWorkflow/Create",
            "TestingAndAnalysisWorkflow/CreateTestingAndAnalysisWorkflow",
            #endregion
            #region 編輯
            "TestingAndAnalysisWorkflow/Edit",
            "TestingAndAnalysisWorkflow/EditTestingAndAnalysisWorkflow",
            #endregion
            #endregion
            
            #region 實驗室標籤管理
            #region 新增
            "LaboratoryLabel_Management/Create",
            "LaboratoryLabel_Management/CreateLaboratoryLabel",
            #endregion
            #region 編輯
            "LaboratoryLabel_Management/Edit",
            "LaboratoryLabel_Management/EditLaboratoryLabel",
            #endregion
            #endregion
            
            #region 實驗室維護管理
            #region 新增
            "LaboratoryMaintenance_Management/Create",
            "LaboratoryMaintenance_Management/CreateLaboratoryMaintenance",
            #endregion
            #region 編輯
            "LaboratoryMaintenance_Management/Edit",
            "LaboratoryMaintenance_Management/EditLaboratoryMaintenance",
            #endregion
            #endregion
            
            #region 實驗數據管理
            #region 新增
            "ExperimentData_Management/Create",
            "ExperimentData_Management/CreateExperimentData",
            #endregion
            #region 編輯
            "ExperimentData_Management/Edit",
            "ExperimentData_Management/EditExperimentData",
            #endregion
            #endregion
            
            #region 警示訊息管理
            #region 填報
            "WarningMessage_Management/Edit",
            "WarningMessage_Management/WarningMessageFillin",
            #endregion
            #endregion
            
            #region 月報管理
            #region 新增
            "MonthlyReport_Management/Create",
            "MonthlyReport_Management/CreateMonthlyReport",
            #endregion
            #region 編輯
            "MonthlyReport_Management/Edit",
            "MonthlyReport_Management/EditMonthlyReport",
            #endregion
            #endregion
            
            #region 會議記錄管理
            #region 新增會議紀錄
            "MeetingMinutes_Management/Create",
            "MeetingMinutes_Management/CreateMeetingMinutes",
            #endregion
            #region 編輯
            "MeetingMinutes_Management/Edit",
            "MeetingMinutes_Management/EditReadBody",
            "MeetingMinutes_Management/EditMeetingMinutes",
            #endregion
            #endregion
        };
    }
}