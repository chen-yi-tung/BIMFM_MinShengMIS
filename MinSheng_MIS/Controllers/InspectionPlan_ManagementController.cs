using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using BorderStyle = NPOI.SS.UserModel.BorderStyle;

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
                var planInfo = db.InspectionPlan.Find(IPSN);

                // 取得所有 PathName
                var pathNames = db.InspectionPlan_Time
                    .Where(x => x.IPSN == IPSN)
                    .Select(x => x.PathName)
                    .Distinct()
                    .ToList();

                foreach (var pathName in pathNames)
                {
                    ISheet sheet = workbook.CreateSheet(pathName);

                    #region 樣式設定
                    //顏色
                    byte[] Primary200 = new byte[] { 200, 224, 244 };
                    XSSFColor Primary200Color = new XSSFColor(Primary200);
                    byte[] Gray50 = new byte[] { 243, 243, 243 };
                    XSSFColor Gray50color = new XSSFColor(Gray50);
                    //粗體
                    IFont boldFont = workbook.CreateFont();
                    boldFont.IsBold = true;
                    boldFont.FontName = "Calibri"; // 設定字型
                    //一般字體
                    IFont font = workbook.CreateFont();
                    font.FontName = "Calibri"; // 設定字型
                    //紅色字體
                    IFont redFont = workbook.CreateFont();
                    redFont.FontName = "Calibri"; // 設定字型
                    redFont.Color = IndexedColors.Red.Index;  // 設定字體為紅色

                    // 設定標題格式
                    XSSFCellStyle TitleStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                    TitleStyle.SetFont(boldFont);
                    TitleStyle.WrapText = true;  // 開啟自動換行
                    TitleStyle.Alignment = HorizontalAlignment.Center;   // 水平置中
                    TitleStyle.VerticalAlignment = VerticalAlignment.Center; // 垂直置中
                    TitleStyle.BorderTop = BorderStyle.Thin;    // 上邊框
                    TitleStyle.BorderBottom = BorderStyle.Thin; // 下邊框
                    TitleStyle.BorderLeft = BorderStyle.Thin;   // 左邊框
                    TitleStyle.BorderRight = BorderStyle.Thin;  // 右邊框
                    TitleStyle.SetFillForegroundColor(Primary200Color);
                    TitleStyle.FillPattern = FillPattern.SolidForeground; // 設定填充模式

                    //設定內文格式
                    //ICellStyle WordStyle = workbook.CreateCellStyle();
                    XSSFCellStyle WordStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                    WordStyle.SetFont(boldFont); //設定為粗體
                    WordStyle.WrapText = true;  // 開啟自動換行
                    WordStyle.Alignment = HorizontalAlignment.Center;   // 水平置中
                    WordStyle.VerticalAlignment = VerticalAlignment.Center; // 垂直置中
                    WordStyle.BorderTop = BorderStyle.Thin;    // 上邊框
                    WordStyle.BorderBottom = BorderStyle.Thin; // 下邊框
                    WordStyle.BorderLeft = BorderStyle.Thin;   // 左邊框
                    WordStyle.BorderRight = BorderStyle.Thin;  // 右邊框
                    WordStyle.SetFillForegroundColor(Gray50color);
                    WordStyle.FillPattern = FillPattern.SolidForeground; // 設定填充模式

                    //設定內容格式
                    ICellStyle ContentStyle = workbook.CreateCellStyle();
                    ContentStyle.SetFont(font);
                    ContentStyle.WrapText = true;  // 開啟自動換行
                    ContentStyle.Alignment = HorizontalAlignment.Center;   // 水平置中
                    ContentStyle.VerticalAlignment = VerticalAlignment.Center; // 垂直置中
                    ContentStyle.BorderTop = BorderStyle.Thin;    // 上邊框
                    ContentStyle.BorderBottom = BorderStyle.Thin; // 下邊框
                    ContentStyle.BorderLeft = BorderStyle.Thin;   // 左邊框
                    ContentStyle.BorderRight = BorderStyle.Thin;  // 右邊框

                    // 設定紅色字體的 Style
                    ICellStyle redTextStyle = workbook.CreateCellStyle();
                    redTextStyle.SetFont(redFont);
                    redTextStyle.WrapText = true;  // 開啟自動換行
                    redTextStyle.Alignment = HorizontalAlignment.Center;   // 水平置中
                    redTextStyle.VerticalAlignment = VerticalAlignment.Center; // 垂直置中
                    redTextStyle.BorderTop = BorderStyle.Thin;    // 上邊框
                    redTextStyle.BorderBottom = BorderStyle.Thin; // 下邊框
                    redTextStyle.BorderLeft = BorderStyle.Thin;   // 左邊框
                    redTextStyle.BorderRight = BorderStyle.Thin;  // 右邊框
                    #endregion

                    //建立標題列
                    IRow row1 = sheet.CreateRow(0);
                    string[] titles = { "工單編號:", "工單名稱:", "工單日期:", "巡檢路線名稱:" };
                    object[] values = { IPSN, planInfo.IPName, planInfo.PlanDate.ToString("yyyy/MM/dd"), pathName };
                    for (int i = 0; i < titles.Length; i++)
                    {
                        row1.CreateCell(i * 2).SetCellValue(titles[i]);
                        row1.GetCell(i * 2).CellStyle = TitleStyle;
                        row1.CreateCell(i * 2 + 1).SetCellValue(values[i]?.ToString());
                        row1.GetCell(i * 2 + 1).CellStyle = WordStyle;
                    }

                    IRow row3 = sheet.CreateRow(2);
                    IRow row4 = sheet.CreateRow(3);
                    sheet.AddMergedRegion(new CellRangeAddress(2, 3, 0, 0)); // 合併 A3:A4

                    row3.CreateCell(0).SetCellValue("設備名稱");
                    row3.GetCell(0).CellStyle = WordStyle;
                    row3.CreateCell(1).SetCellValue("開始時間");
                    row3.GetCell(1).CellStyle = WordStyle;
                    row4.CreateCell(1).SetCellValue("結束時間");
                    row4.GetCell(1).CellStyle = WordStyle;
                    

                    // 取得巡檢計畫資料
                    var datas = db.InspectionPlan_Time.Where(x => x.PathName == pathName && x.IPSN == IPSN).ToList();
                    var maxcell = 7;
                    if (datas.Count > 0)
                    {
                        var iptsn = db.InspectionPlan_Time.Where(x => x.PathName == pathName && x.IPSN == IPSN && x.InspectionState != "1").Select(x => x.IPTSN).FirstOrDefault().ToString();

                        // 取得設備資料
                        var equipments = (from x1 in db.InspectionPlan_Equipment
                                         where x1.IPTSN == iptsn
                                         join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                                         select new { x1.ESN, x1.IPESN, x2.EName, x2.NO }).ToList();

                        var rowIndex = 4;
                        foreach (var equipment in equipments)
                        {
                            string eqName = equipment.EName + " " + equipment.NO; // 設備名稱 + 編號
                            var checkItems = db.InspectionPlan_EquipmentCheckItem.Where(x => x.IPESN == equipment.IPESN).ToList();
                            var reportingItems = db.InspectionPlan_EquipmentReportingItem.Where(x => x.IPESN == equipment.IPESN).ToList();
                            int count = checkItems.Count + reportingItems.Count;

                            // 合併儲存格 & 設定設備名稱
                            if (checkItems.Any() || reportingItems.Any())
                            {
                                sheet.CreateRow(rowIndex).CreateCell(0).SetCellValue(eqName);
                                sheet.GetRow(rowIndex).GetCell(0).CellStyle = WordStyle;
                                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex + count - 1, 0, 0));
                                var BrowIndex = rowIndex;
                                //檢查項目
                                foreach (var item in checkItems)
                                {
                                    IRow currentRow = sheet.GetRow(BrowIndex) ?? sheet.CreateRow(BrowIndex);
                                    currentRow.CreateCell(1).SetCellValue(item.CheckItemName);

                                    sheet.GetRow(BrowIndex).GetCell(1).CellStyle = WordStyle;
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
                                    sheet.GetRow(BrowIndex).GetCell(1).CellStyle = WordStyle;
                                    BrowIndex++;
                                }
                                rowIndex = rowIndex + count;
                            }
                        }
                        sheet.CreateRow(rowIndex).CreateCell(1).SetCellValue("執行人員");
                        sheet.GetRow(rowIndex).GetCell(1).CellStyle = WordStyle;
                        sheet.GetRow(rowIndex).CreateCell(0).CellStyle = WordStyle;
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
                                        if(item.CheckResult == "2") //異常
                                        {
                                            sheet.GetRow(reportrowIndex).GetCell(recordColumnIndex).CellStyle = redTextStyle;
                                        }
                                        else //正常
                                        {
                                            sheet.GetRow(reportrowIndex).GetCell(recordColumnIndex).CellStyle = ContentStyle;
                                        }
                                    }
                                    else
                                    {
                                        sheet.GetRow(reportrowIndex).CreateCell(recordColumnIndex).SetCellValue("");
                                        sheet.GetRow(reportrowIndex).GetCell(recordColumnIndex).CellStyle = ContentStyle;
                                    }
                                    reportrowIndex++;
                                }
                                var Reportingitems = db.InspectionPlan_EquipmentReportingItem.Where(x => x.IPESN == e.IPESN).ToList();
                                foreach (var item in Reportingitems)
                                {
                                    sheet.GetRow(reportrowIndex).CreateCell(recordColumnIndex).SetCellValue(item.ReportContent);
                                    sheet.GetRow(reportrowIndex).GetCell(recordColumnIndex).CellStyle = ContentStyle;
                                    reportrowIndex++;
                                }
                            }
                            //執行人員
                            var inspectionMembers = string.Join("、", db.InspectionPlan_Member
                                                          .Where(x => x.IPTSN == data.IPTSN)
                                                          .Join(db.AspNetUsers, m => m.UserID, u => u.UserName, (m, u) => u.MyName)
                                                          .ToList());

                            sheet.GetRow(rowIndex).CreateCell(recordColumnIndex).SetCellValue(inspectionMembers);
                            sheet.GetRow(rowIndex).GetCell(recordColumnIndex).CellStyle = ContentStyle;
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
                            row3.GetCell(columnIndex).CellStyle = WordStyle;
                            row4.CreateCell(columnIndex).SetCellValue(data.EndTime.ToString());
                            row4.GetCell(columnIndex).CellStyle = WordStyle;
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
                /*
                // **🔹 設定下載目標路徑**
                string folderPath = Path.Combine(Server.MapPath("~"), "Downloads");
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
                }*/

                // **🔹 讓使用者下載 Excel 檔案**
                //return File(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                using (MemoryStream ms = new MemoryStream())
                {
                    workbook.Write(ms);
                    byte[] fileBytes = ms.ToArray();
                    string base64String = Convert.ToBase64String(fileBytes);

                    // **🔹 回傳 JSON**
                    return Json(new
                    {
                        success = true,
                        message = "Excel生成成功",
                        fileName = IPSN+".xlsx",
                        fileData = base64String
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}