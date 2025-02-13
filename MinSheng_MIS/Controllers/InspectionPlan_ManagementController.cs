using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class InspectionPlan_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
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
            #region 當日
            DateTime StartDate = DateTime.Today.Date;
            DateTime EndDate = DateTime.Today.AddDays(1);
            #endregion
            #region 當月
            DateTime currentDate = DateTime.Now;
            DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1);
            #endregion
            try
            {
                using (PlanInformationService ds = new PlanInformationService())
                {
                    jo["ChartInspectionCompleteState"] = ds.ChartInspectionCompleteState(StartDate, EndDate); //當日
                    jo["ChartInspectionEquipmentState"] = ds.ChartInspectionEquipmentState(StartDate, EndDate); //當日
                    jo["EquipmentOperatingState"] = ds.EquipmentOperatingState(FSN);
                    jo["EnvironmentInfo"] = ds.EnvironmentInfo(FSN);
                    jo["ChartInspectionAberrantLevel"] = ds.ChartInspectionAberrantLevel(firstDayOfMonth, lastDayOfMonth); //當月
                    jo["ChartInspectionAberrantResolve"] = ds.ChartInspectionAberrantResolve(firstDayOfMonth, lastDayOfMonth); //當月
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

        #region 取得巡檢結果
        [HttpGet]
        public ActionResult GetInspectionExcel(string IPSN)
        {
            JObject jo = new JObject();
            var CheckResult_Dic = Surface.CheckResult();
            try
            {
                IWorkbook workbook = new XSSFWorkbook();

                // 取得Plan資訊
                var planInfo = db.InspectionPlan
                    .Find(IPSN);
                // 取得所有 PathName
                var pathNames = db.InspectionPlan_Time
                    .Where(x => x.IPSN == IPSN)
                    .Select(x => x.PathName)
                    .Distinct()
                    .ToList();

                foreach (var pathName in pathNames)
                {
                    ISheet sheet = workbook.CreateSheet(pathName);

                    // 設定標題格式
                    ICellStyle boldStyle = workbook.CreateCellStyle();
                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    boldStyle.SetFont(font);
                    boldStyle.WrapText = true;  // 開啟自動換行

                    //設定內文格式
                    ICellStyle WordStyle = workbook.CreateCellStyle();
                    WordStyle.WrapText = true;  // 開啟自動換行
                    WordStyle.Alignment = HorizontalAlignment.Center;   // 水平置中
                    WordStyle.VerticalAlignment = VerticalAlignment.Center; // 垂直置中

                    //建立標題列
                    IRow row1 = sheet.CreateRow(0);
                    row1.CreateCell(0).SetCellValue("工單編號:");                 
                    row1.CreateCell(2).SetCellValue("工單名稱:");                   
                    row1.CreateCell(4).SetCellValue("工單日期:");
                    row1.CreateCell(6).SetCellValue("巡檢路線名稱:");
                    row1.Cells.ForEach(c => c.CellStyle = boldStyle); 

                    row1.CreateCell(1).SetCellValue(IPSN);
                    row1.CreateCell(3).SetCellValue(planInfo.IPName);
                    row1.CreateCell(5).SetCellValue(planInfo.PlanDate.ToString("yyyy/MM/dd"));
                    row1.CreateCell(7).SetCellValue(pathName);
                    row1.Cells.ForEach(c => c.CellStyle = WordStyle);

                    IRow row3 = sheet.CreateRow(2);
                    ICell cellA3 = row3.CreateCell(0);
                    cellA3.SetCellValue("設備名稱");
                    sheet.AddMergedRegion(new CellRangeAddress(2, 3, 0, 0)); // 合併 A3:A4
                    row3.CreateCell(1).SetCellValue("開始時間");
                    IRow row4 = sheet.CreateRow(3);
                    row4.CreateCell(1).SetCellValue("結束時間");
                    

                    // 取得巡檢計畫資料
                    var datas = db.InspectionPlan_Time.Where(x => x.PathName == pathName && x.IPSN == IPSN).ToList();
                    var maxcell = 7;
                    if (datas.Count > 0)
                    {
                        var iptsn = datas[0].IPTSN;

                        // 取得設備資料
                        var equipments = db.InspectionPlan_Equipment.Where(x => x.IPTSN == iptsn).ToList();
                        
                        var rowIndex = 4;
                        foreach (var equipment in equipments)
                        {
                            var eq = db.EquipmentInfo.Find(equipment.ESN);
                            string eqName = eq.EName + eq.NO; // 設備名稱 + 編號

                            var checkItems = db.InspectionPlan_EquipmentCheckItem.Where(x => x.IPESN == equipment.IPESN).ToList();
                            var reportingItems = db.InspectionPlan_EquipmentReportingItem.Where(x => x.IPESN == equipment.IPESN).ToList();
                            int count = checkItems.Count + reportingItems.Count;

                            // 合併儲存格 & 設定設備名稱
                            if (count > 0)
                            {
                                sheet.CreateRow(rowIndex).CreateCell(0).SetCellValue(eqName);
                                var BrowIndex = rowIndex;
                                //檢查項目
                                foreach (var item in checkItems)
                                {
                                    if(BrowIndex == rowIndex)
                                    {
                                        sheet.GetRow(BrowIndex).CreateCell(1).SetCellValue(item.CheckItemName);
                                    }
                                    else
                                    {
                                        sheet.CreateRow(BrowIndex).CreateCell(1).SetCellValue(item.CheckItemName);
                                    }
                                    BrowIndex++;
                                }
                                foreach (var item in reportingItems)
                                {
                                    if(BrowIndex == rowIndex)
                                    {
                                        sheet.GetRow(BrowIndex).CreateCell(1).SetCellValue((item.ReportValue + "(" + item.Unit + ")"));
                                    }
                                    else
                                    {
                                        sheet.CreateRow(BrowIndex).CreateCell(1).SetCellValue((item.ReportValue + "(" + item.Unit + ")"));
                                    }
                                    BrowIndex++;
                                }
                                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex + count-1, 0, 0));
                                rowIndex = rowIndex + count;
                            }
                        }
                        sheet.CreateRow(rowIndex).CreateCell(1).SetCellValue("執行人員");
                        //依時段填檢查項目/填報項目
                        var recordColumnIndex = 2;
                        foreach (var data in datas)
                        {
                            var reportrowIndex = 4;
                            var eqs = db.InspectionPlan_Equipment.Where(x => x.IPTSN == data.IPTSN).ToList();
                            foreach (var e in eqs)
                            {
                                var Checkitems = db.InspectionPlan_EquipmentCheckItem.Where(x => x.IPESN == e.IPESN).ToList();
                                foreach (var item in Checkitems)
                                {
                                    if(item.CheckResult != null)
                                    {
                                        sheet.GetRow(reportrowIndex).CreateCell(recordColumnIndex).SetCellValue(CheckResult_Dic[item.CheckResult]);
                                    }
                                    reportrowIndex++;
                                }
                                var Reportingitems = db.InspectionPlan_EquipmentReportingItem.Where(x => x.IPESN == e.IPESN).ToList();
                                foreach (var item in Reportingitems)
                                {
                                    sheet.GetRow(reportrowIndex).CreateCell(recordColumnIndex).SetCellValue(item.ReportContent);
                                    reportrowIndex++;
                                }
                            }
                            //執行人員
                            
                            var members = (from x1 in db.InspectionPlan_Member
                                           where x1.IPTSN == data.IPTSN
                                          join x2 in db.AspNetUsers on x1.UserID equals x2.UserName
                                          select new { x2.MyName}).ToList();
                            var inspectionmembers = "";
                            for(int i = 0;i < members.Count(); i++)
                            {
                                if (i != 0)
                                {
                                    inspectionmembers += "、";
                                }
                                inspectionmembers += members[i].MyName.ToString();
                            }

                            sheet.GetRow(reportrowIndex).CreateCell(recordColumnIndex).SetCellValue(inspectionmembers);
                            recordColumnIndex++;
                        }
                        if(maxcell< recordColumnIndex)
                        {
                            maxcell = recordColumnIndex;
                        }
                        // 設定開始時間 & 結束時間
                        int columnIndex = 2;
                        foreach (var data in datas)
                        {
                            row3.CreateCell(columnIndex).SetCellValue(data.StartTime.ToString());
                            row4.CreateCell(columnIndex).SetCellValue(data.EndTime.ToString());
                            sheet.SetColumnWidth(columnIndex, 20 * 256);
                            columnIndex++;
                        }
                    }

                    // 欄位寬度
                    sheet.SetColumnWidth(0, 30 * 256);
                    sheet.SetColumnWidth(1, 30 * 256);
                    for (int col = 2; col <= maxcell; col++)
                    {
                        sheet.SetColumnWidth(col, 25 * 256);

                    }
                }
                // **🔹 設定下載目標路徑**
                string folderPath = Server.MapPath("~/Downloads/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string fileName = "巡檢結果.xlsx";
                string filePath = Path.Combine(folderPath, fileName);

                // **🔹 將 Excel 檔案存到本地**
                using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(stream);
                }

                // **🔹 讓使用者下載 Excel 檔案**
                return File(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}