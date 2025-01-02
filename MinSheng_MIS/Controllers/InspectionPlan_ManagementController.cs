using MinSheng_MIS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class InspectionPlan_ManagementController : Controller
    {

        #region 巡檢即時位置
        public ActionResult CurrentPosition()
        {
            return View();
        }
        #endregion

        #region 巡檢資訊管理
        public ActionResult InformationManagement()
        {
            return View();
        }
        #endregion

        #region WEB API 

        #region 取得 巡檢即時位置 資訊
        [HttpGet]
        public ActionResult GetCurrentPositionData(string FSN)
        {
            JObject jo = new JObject();
            DateTime StartDate = DateTime.Today;
            DateTime EndDate = DateTime.Today.AddDays(1);
            try
            {
                using (PlanInformationService ds = new PlanInformationService())
                {
                    jo["ChartInspectionCompleteState"] = ds.ChartInspectionCompleteState(StartDate, EndDate);
                    jo["ChartInspectionEquipmentState"] = ds.ChartInspectionEquipmentState(StartDate, EndDate);
                    jo["EquipmentOperatingState"] = ds.EquipmentOperatingState(FSN);
                    jo["EnvironmentInfo"] = ds.EnvironmentInfo(FSN);
                    jo["ChartInspectionAberrantLevel"] = ds.ChartInspectionAberrantLevel(StartDate, EndDate);
                    jo["ChartInspectionAberrantResolve"] = ds.ChartInspectionAberrantResolve(StartDate, EndDate);
                    jo["InspectionCurrentPos"] = ds.InspectionCurrentPos(FSN);
                }
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 取得 巡檢資訊管理 資訊
        [HttpGet]
        public ActionResult GetInspectionPlanInformation(string year1, string month1, string year2, string month2)
        {
            JObject jo = new JObject();

            DateTime StartDate;
            DateTime EndDate;

            try
            {
                if (
                    int.TryParse(year1, out int StartYear) &&
                    int.TryParse(month1, out int StartMonth) &&
                    int.TryParse(year2, out int EndYear) &&
                    int.TryParse(month2, out int EndMonth) &&
                    StartMonth >= 1 && StartMonth <= 12 &&
                    EndMonth >= 1 && EndMonth <= 12
                )
                {
                    StartDate = new DateTime(StartYear, StartMonth, 1);

                    int lastDay = DateTime.DaysInMonth(EndYear, EndMonth);
                    EndDate = new DateTime(EndYear, EndMonth, lastDay).AddDays(1);
                }
                else
                {
                    throw new Exception("輸入的年或月格式無效或超出範圍。");
                }

                using (PlanInformationService ds = new PlanInformationService())
                {
                    jo["ChartInspectionCompleteState"] = ds.ChartInspectionCompleteState(StartDate, EndDate);
                    jo["ChartInspectionEquipmentState"] = ds.ChartInspectionEquipmentState(StartDate, EndDate);
                    jo["InspectionMembers"] = ds.InspectionMembers(StartDate, EndDate);
                    jo["ChartInspectionAberrantLevel"] = ds.ChartInspectionAberrantLevel(StartDate, EndDate);
                    jo["ChartInspectionAberrantResolve"] = ds.ChartInspectionAberrantResolve(StartDate, EndDate);
                    jo["ChartEquipmentProgressStatistics"] = ds.ChartEquipmentProgressStatistics(StartDate, EndDate);
                    jo["ChartEquipmentLevelRate"] = ds.ChartEquipmentLevelRate(StartDate, EndDate);
                    jo["ChartEquipmentTypeRate"] = ds.ChartEquipmentTypeRate(StartDate, EndDate);
                }
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #endregion
    }
}