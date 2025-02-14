using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.Util;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using SixLabors.ImageSharp.PixelFormats;
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

        #region 取得巡檢結果Excel
        [HttpGet]
        public ActionResult GetInspectionExcel(string IPSN)
        {
            JObject jo = new JObject();
            var CheckResult_Dic = Surface.CheckResult();
            try
            {
                // 取得Plan資訊
                var planInfo = db.InspectionPlan.Find(IPSN);

                //檢查工單是否有執行
                if(planInfo.PlanState.ToString() == "1") //待執行則不可下載紀錄
                {
                    return Json(new { success = false, message = "此巡檢計畫(" + IPSN + ")尚未開始巡檢，因此無巡檢紀錄可以下載" }, JsonRequestBehavior.AllowGet);
                }


                IWorkbook workbook = new XSSFWorkbook();

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
                    XSSFColor Primary200Color = new XSSFColor(new byte[] { 200, 224, 244 });
                    XSSFColor Primary150Color = new XSSFColor(new byte[] { 222, 235, 246 });
                    XSSFColor Gray50Color = new XSSFColor(new byte[] { 243, 243, 243 });

                    //文字設定
                    IFont CreateFont(IWorkbook workbooki, bool isBold = false, bool isRed = false)
                    {
                        IFont font = workbook.CreateFont();
                        font.FontName = "Calibri";
                        font.IsBold = isBold;
                        if (isRed)
                        {
                            font.Color = IndexedColors.Red.Index;
                        }
                        return font;
                    }

                    // 創建樣式函式
                    ICellStyle CreateCellStyle(IWorkbook workbookj, IFont font, XSSFColor bgColor = null)
                    {
                        XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();
                        style.SetFont(font);
                        style.WrapText = true;//自動換行
                        style.Alignment = HorizontalAlignment.Center;
                        style.VerticalAlignment = VerticalAlignment.Center;
                        style.BorderTop = BorderStyle.Thin;
                        style.BorderBottom = BorderStyle.Thin;
                        style.BorderLeft = BorderStyle.Thin;
                        style.BorderRight = BorderStyle.Thin;

                        if (bgColor != null)
                        {
                            style.SetFillForegroundColor(bgColor);
                            style.FillPattern = FillPattern.SolidForeground;
                        }

                        return style;
                    }

                    // 創建字體
                    IFont boldFont = CreateFont(workbook, isBold: true);
                    IFont normalFont = CreateFont(workbook);
                    IFont redFont = CreateFont(workbook, isRed: true);

                    // 創建樣式
                    ICellStyle TitleStyle = CreateCellStyle(workbook, boldFont, Primary200Color);//標題樣式
                    ICellStyle SubTitleStyle = CreateCellStyle(workbook, boldFont, Primary150Color);//標題樣式
                    ICellStyle WordStyle = CreateCellStyle(workbook, boldFont, Gray50Color);//樣式
                    ICellStyle ContentStyle = CreateCellStyle(workbook, normalFont);//填報內容樣式
                    ICellStyle RedTextStyle = CreateCellStyle(workbook, redFont);//異常內容樣式
                    #endregion

                    #region 巡檢基本資料
                    //建立標題列
                    string[] titles = { "工單編號", "工單名稱", "工單日期", "巡檢路線名稱" };
                    object[] values = { IPSN, planInfo.IPName, planInfo.PlanDate.ToString("yyyy/MM/dd"), pathName };
                    for (int i = 0; i < titles.Length; i++)
                    {
                        sheet.CreateRow(i).CreateCell(0).SetCellValue(titles[i]);
                        sheet.GetRow(i).GetCell(0).CellStyle = TitleStyle;
                        sheet.GetRow(i).CreateCell(1).SetCellValue(values[i].ToString());
                        sheet.GetRow(i).GetCell(1).CellStyle = WordStyle;
                    }
                    #endregion

                    //IRow row5 = sheet.CreateRow(4);
                    //IRow row6 = sheet.CreateRow(5);
                    

                    SetCellValueWithStyle(sheet, 4, 0, "設備名稱", SubTitleStyle);
                    SetCellValueWithStyle(sheet, 4, 1, "開始時間", SubTitleStyle);
                    SetCellValueWithStyle(sheet, 5, 1, "結束時間", SubTitleStyle);
                    sheet.AddMergedRegion(new CellRangeAddress(4, 5, 0, 0)); // 合併 A3:A4

                    // 取得巡檢計畫資料
                    var datas = db.InspectionPlan_Time.Where(x => x.PathName == pathName && x.IPSN == IPSN).ToList();
                    if (datas.Count > 0)
                    {
                        #region 填入設備名稱+編號、填報項目 => A欄、B欄
                        var iptsn = db.InspectionPlan_Time.Where(x => x.PathName == pathName && x.IPSN == IPSN && x.InspectionState != "1").Select(x => x.IPTSN).FirstOrDefault().ToString();

                        // 取得設備資料
                        var equipments = (from x1 in db.InspectionPlan_Equipment
                                         where x1.IPTSN == iptsn
                                         join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                                         select new { x1.ESN, x1.IPESN, x2.EName, x2.NO }).ToList();
                        int equipmentsCount = equipments.Count();
                        var rowIndex = 6;
                        int totalCount = 0;
                        foreach (var equipment in equipments)
                        {
                            string eqName = equipment.EName + " " + equipment.NO; // 設備名稱 + 編號
                            var checkItems = db.InspectionPlan_EquipmentCheckItem.Where(x => x.IPESN == equipment.IPESN).ToList();
                            var reportingItems = db.InspectionPlan_EquipmentReportingItem.Where(x => x.IPESN == equipment.IPESN).ToList();
                            int count = checkItems.Count + reportingItems.Count;
                            totalCount += count;
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
                        #endregion

                        #region 依時段填檢查項目、填報項目
                        var recordColumnIndex = 2;
                        foreach (var data in datas)
                        {
                            var reportrowIndex = 6;
                            //先畫好框框
                            for (int i = reportrowIndex; i < reportrowIndex + totalCount; i++)
                            {
                                sheet.GetRow(i).CreateCell(recordColumnIndex).SetCellValue("");
                                sheet.GetRow(i).GetCell(recordColumnIndex).CellStyle = ContentStyle;
                            }
                            var eqs = db.InspectionPlan_Equipment.Where(x => x.IPTSN == data.IPTSN).ToList();
                            foreach (var e in eqs)
                            {
                                var Checkitems = db.InspectionPlan_EquipmentCheckItem.Where(x => x.IPESN == e.IPESN).ToList();
                                foreach (var item in Checkitems)
                                {
                                    if(item.CheckResult != null)
                                    {
                                        sheet.GetRow(reportrowIndex).GetCell(recordColumnIndex).SetCellValue(CheckResult_Dic[item.CheckResult]);
                                        if(item.CheckResult == "2") //異常
                                        {
                                            sheet.GetRow(reportrowIndex).GetCell(recordColumnIndex).CellStyle = RedTextStyle;
                                        }
                                    }
                                    else
                                    {
                                        sheet.GetRow(reportrowIndex).GetCell(recordColumnIndex).SetCellValue("");
                                    }
                                    reportrowIndex++;
                                }
                                var Reportingitems = db.InspectionPlan_EquipmentReportingItem.Where(x => x.IPESN == e.IPESN).ToList();
                                foreach (var item in Reportingitems)
                                {
                                    sheet.GetRow(reportrowIndex).GetCell(recordColumnIndex).SetCellValue(item.ReportContent);
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
                        #endregion

                        #region 開始時間、結束時間
                        //設定開始時間 & 結束時間
                        int columnIndex = 2;
                        foreach (var data in datas)
                        {
                            SetCellValueWithStyle(sheet, 4, columnIndex, data.StartTime.ToString(), SubTitleStyle);
                            SetCellValueWithStyle(sheet, 5, columnIndex, data.EndTime.ToString(), SubTitleStyle);
                            columnIndex++;
                        }
                        #endregion
                    }
                    #region 欄位寬度
                    int[] wideColumns = { 0, 1 };
                    foreach (int col in wideColumns)
                    {
                        sheet.SetColumnWidth(col, 30 * 256);
                    }
                    for (int col = 2; col <= datas.Count()+1; col++)
                    {
                        sheet.SetColumnWidth(col, 10 * 256);
                    }
                    #endregion
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
        void SetCellValueWithStyle(ISheet sheet, int rowIndex, int columnIndex, string value, ICellStyle style)
        {
            // 取得或建立 Row
            IRow row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);

            // 取得或建立 Cell
            ICell cell = row.GetCell(columnIndex) ?? row.CreateCell(columnIndex);

            // 設定值與樣式
            cell.SetCellValue(value);
            cell.CellStyle = style;
        }
    }

}