using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System.Linq.Dynamic.Core;
using System.Data.Entity.Migrations;
using MinSheng_MIS.Surfaces;
using System.Data.Entity;
using static MinSheng_MIS.Models.ViewModels.MaintainManagementApp_ListViewModel;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml.FormulaParsing.Excel.Functions;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using System.Web.Mvc;
using MiniExcelLibs;
using System.Windows;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MinSheng_MIS.Services
{
    public class Maintain_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;

        public Maintain_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        // WEB
        #region 定期保養單 詳情
        public JsonResService<Maintain_ManagementDetailViewModel> MaintainManagement_Details(string emfsn)
        {
            #region 變數
            JsonResService<Maintain_ManagementDetailViewModel> res = new JsonResService<Maintain_ManagementDetailViewModel>();
            #endregion

            #region 資料檢查
            var maindata = _db.Equipment_MaintenanceForm.Find(emfsn);

            if (maindata == null)
                throw new MyCusResException("查無此設備保養單");
            #endregion

            #region 塞資料
            Maintain_ManagementDetailViewModel datas = new Maintain_ManagementDetailViewModel();
            datas.EMFSN = maindata.EMFSN;
            datas.Status = Surface.MaintainStatus()[maindata.Status];
            datas.MaintainName = maindata.MaintainName;
            datas.Maintainer = string.Join("、", maindata.Equipment_MaintenanceFormMember.Select(x => x.Maintainer));
            datas.ESN = maindata.ESN;
            datas.EName = maindata.EquipmentInfo.EName;
            datas.EState = Surface.EState()[maindata.EquipmentInfo.EState];
            datas.NO = maindata.EquipmentInfo.NO;
            datas.Location = $"{maindata.EquipmentInfo.Floor_Info.FloorName} {maindata.EquipmentInfo.Floor_Info.AreaInfo.Area}";
            datas.Period = Surface.MaintainPeriod()[maindata.Period];
            datas.LastMaintainDate = maindata.lastMaintainDate?.ToString("yyyy-MM-dd") ?? "-";
            datas.NextMaintainDate = maindata.NextMaintainDate.ToString("yyyy-MM-dd");
            datas.ReportId = GetMyNameByUserNameOrEmpty(maindata.ReportId);
            datas.ReportTime = maindata.ReportTime?.ToString("yyyy-MM-dd HH:mm") ?? "-";
            datas.ReportContent = maindata.ReportContent;
            datas.AuditResult = maindata.AuditResult.ToString().ToLower();
            datas.AuditId = GetMyNameByUserNameOrEmpty(maindata.AuditId);
            datas.AuditTime = maindata.AuditTime?.ToString("yyyy-MM-dd HH:mm") ?? "-";
            datas.AuditReason = maindata.AuditReason;
            res.Datas = datas;
            #endregion

            res.AccessState = ResState.Success;
            return res;
        }
        #endregion

        #region 定期保養單 派工
        public JsonResService<string> MaintainManagement_Assignment(Maintain_ManagementAssignmentViewModel datas, string userName)
        {
            JsonResService<string> res = new JsonResService<string>();

            #region 更新保養單資料、新增保養單派工
            foreach (var emfsn in datas.EMFSN)
            {
                var emfsndata = _db.Equipment_MaintenanceForm.Where(x => x.EMFSN == emfsn && x.Status == "1").FirstOrDefault();
                if (emfsndata == null)
                    throw new MyCusResException("查無此定期保養單");

                emfsndata.NextMaintainDate = ToAD(datas.NextMaintainDate).Value;
                emfsndata.Status = "2"; // 待執行
                emfsndata.Dispatcher = userName;
                emfsndata.DispatcherTime = DateTime.Now;
                var lastsn = _db.Equipment_MaintenanceFormMember.Where(x => x.EMFSN == emfsn)
                        .OrderByDescending(x => x.EMFMSN).Select(x => x.EMFMSN).FirstOrDefault();
                foreach (var maintainer in datas.Maintainer)
                {
                    lastsn = ComFunc.CreateNextID(emfsn + "%{2}", lastsn);
                    var emfmsndata = new Equipment_MaintenanceFormMember();
                    emfmsndata.EMFMSN = lastsn;
                    emfmsndata.EMFSN = emfsn;
                    emfmsndata.Maintainer = maintainer;
                    _db.Equipment_MaintenanceFormMember.Add(emfmsndata);
                }
            }
            _db.SaveChanges();
            #endregion

            res.AccessState = ResState.Success;
            res.Datas = "派工成功!";
            return res;
        }
        #endregion

        #region 定期保養單 審核
        public JsonResService<string> MaintainManagement_Audit(Maintain_ManagementAuditViewModel datas, string userName)
        {
            JsonResService<string> res = new JsonResService<string>();

            #region 檢查資料
            var maindata = _db.Equipment_MaintenanceForm.Where(x => x.EMFSN == datas.EMFSN && x.Status == "3").FirstOrDefault();
            if (maindata == null)
                throw new MyCusResException("查無此設備保養單");
            if (string.IsNullOrEmpty(datas.AuditReason.Trim()))
                throw new MyCusResException("請填寫審核意見內容!");

            var maintainvalue = _db.Equipment_MaintainItemValue.Find($"{maindata.ESN}{maindata.MISSN}");
            if (maintainvalue == null)
                throw new Exception("查無設備保養項目");
            #endregion

            #region 更新資料
            if (datas.AuditResult)
            {
                // 更新設備保養單
                maindata.Status = "4"; // 審核通過
                maindata.MISSN = null;

                // 更新設備保養項目填值
                maintainvalue.IsCreateForm = false;
                maintainvalue.lastMaintainDate = DateTime.Now;
                maintainvalue.NextMaintainDate = UniParams.GetNextMaintainDate(maintainvalue.Period);
                if (CheckIfCreateMaintainForm(maintainvalue))
                    CreateMaintainForm(maintainvalue);
            }
            else maindata.Status = "5"; // 審核未過

            maindata.AuditId = userName;
            maindata.AuditTime = DateTime.Now;
            maindata.AuditResult = datas.AuditResult;
            maindata.AuditReason = datas.AuditReason;

            _db.Equipment_MaintenanceForm.AddOrUpdate(maindata);
            _db.SaveChanges();
            #endregion

            res.AccessState = ResState.Success;
            res.Datas = "填報成功!";
            return res;
        }
        #endregion

        #region 定期保養單 匯出
        public MemoryStream MaintainManagement_Export(FormCollection form)
        {
            form.Add("rows", _db.Equipment_MaintenanceForm.Count().ToString());

            DatagridService datagridServ = new DatagridService();
            JObject datagridjson = datagridServ.GetJsonForGrid_MaintainForm(form);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("保養單管理");

                worksheet.Cells["A1"].Value = "保養單狀態";
                worksheet.Cells["B1"].Value = "保養單號";
                worksheet.Cells["C1"].Value = "最近應保養日期";
                worksheet.Cells["D1"].Value = "實際保養日期";
                worksheet.Cells["E1"].Value = "設備狀態";
                worksheet.Cells["F1"].Value = "棟別";
                worksheet.Cells["G1"].Value = "樓層";
                worksheet.Cells["H1"].Value = "設備名稱";
                worksheet.Cells["I1"].Value = "設備編號";
                worksheet.Cells["J1"].Value = "保養項目";
                worksheet.Cells["K1"].Value = "保養週期";
                worksheet.Cells["L1"].Value = "執行人員";

                int row = 2;
                foreach (var item in datagridjson["rows"])
                {
                    worksheet.Cells["A" + row].Value = item["Status"].ToString();
                    worksheet.Cells["B" + row].Value = item["EMFSN"].ToString();
                    worksheet.Cells["C" + row].Value = item["NextMaintainDate"].ToString();
                    worksheet.Cells["D" + row].Value = item["ReportTime"].ToString();
                    worksheet.Cells["E" + row].Value = item["EState"].ToString();
                    worksheet.Cells["F" + row].Value = item["Area"].ToString();
                    worksheet.Cells["G" + row].Value = item["FloorName"].ToString();
                    worksheet.Cells["H" + row].Value = item["EName"].ToString();
                    worksheet.Cells["I" + row].Value = item["ESN"].ToString();
                    worksheet.Cells["J" + row].Value = item["MaintainName"].ToString();
                    worksheet.Cells["K" + row].Value = item["Period"].ToString();
                    worksheet.Cells["L" + row].Value = item["Maintainer"].ToString();
                    row++;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
        }
        #endregion

        // APP
        #region 定期保養單 列表
        public JsonResService<MaintainManagementApp_ListViewModel> MaintainManagementApp_List(string Status, string userName)
        {
            #region 變數
            JsonResService<MaintainManagementApp_ListViewModel> res = new JsonResService<MaintainManagementApp_ListViewModel>();
            MaintainManagementApp_ListViewModel resdata = new MaintainManagementApp_ListViewModel();
            List<MaintainManagementApp_ListItem> maintainlist = new List<MaintainManagementApp_ListItem>();
            #endregion

            #region 查資料
            var maindata = _db.Equipment_MaintenanceForm.Where(x => x.Equipment_MaintenanceFormMember.Any(m => m.Maintainer == userName) && x.Status == "2" || x.Status == "5").AsNoTracking().ToList();
            int totalNum = maindata.Count; // 全部
            int pendingNum = maindata.Where(x => x.Status == "2").Count(); // 待執行
            int notApprovedNum = maindata.Where(x => x.Status == "5").Count(); // 審核未過

            if (Status == "Pending") // 待執行
                maindata = maindata.Where(x => x.Status == "2").ToList();
            else if (Status == "NotApproved") // 審核未過
                maindata = maindata.Where(x => x.Status == "5").ToList();

            maindata = maindata.OrderBy(x => x.NextMaintainDate).ThenBy(x=>x.DispatcherTime).ToList();
            #endregion

            #region 塞資料
            resdata.TotalNum = totalNum.ToString();
            resdata.PendingNum = pendingNum.ToString();
            resdata.NotApprovedNum = notApprovedNum.ToString();

            foreach (var maintainform in maindata)
            {
                MaintainManagementApp_ListItem resdataitem = new MaintainManagementApp_ListItem();
                resdataitem.RFIDList = _db.RFID.Where(x => x.ESN == maintainform.ESN).Select(x => x.RFIDInternalCode).AsNoTracking().ToList();
                resdataitem.EMFSN = maintainform.EMFSN;
                resdataitem.Status = Surface.MaintainStatus()[maintainform.Status];
                resdataitem.EName = maintainform.EquipmentInfo.EName;
                resdataitem.NO = maintainform.EquipmentInfo.NO;
                resdataitem.EState = Surface.EState()[maintainform.EquipmentInfo.EState];
                resdataitem.Area = maintainform.EquipmentInfo.Floor_Info.AreaInfo.Area;
                resdataitem.FloorName = maintainform.EquipmentInfo.Floor_Info.FloorName;
                resdataitem.DispatcherTime = maintainform.DispatcherTime?.ToString("yyyy-MM-dd");
                resdataitem.NextMaintainDate = maintainform.NextMaintainDate.ToString("yyyy-MM-dd");

                maintainlist.Add(resdataitem);
            }
            resdata.MaintainFormList = maintainlist;
            #endregion

            res.AccessState = ResState.Success;
            res.Datas = resdata;
            return res;
        }
        #endregion

        #region 定期保養單 填報詳情
        public JsonResService<MaintainManagementApp_Detail> MaintainManagementApp_Detail(string EMFSN, string userName)
        {
            #region 變數
            JsonResService<MaintainManagementApp_Detail> res = new JsonResService<MaintainManagementApp_Detail>();
            MaintainManagementApp_Detail resdata = new MaintainManagementApp_Detail();
            #endregion

            #region 塞資料
            var maindata = _db.Equipment_MaintenanceForm
                .Where(x => (x.Status == "2" || x.Status == "5") 
                && x.Equipment_MaintenanceFormMember.Any(m => m.Maintainer == userName) && x.EMFSN == EMFSN).FirstOrDefault();
            if (maindata == null)
                throw new MyCusResException("查無此設備保養單");

            resdata.EMFSN = maindata.EMFSN;
            resdata.EName = maindata.EquipmentInfo.EName;
            resdata.NO = maindata.EquipmentInfo.NO;
            resdata.Area = maindata.EquipmentInfo.Floor_Info.AreaInfo.Area;
            resdata.FloorName = maindata.EquipmentInfo.Floor_Info.FloorName;
            resdata.ESN = maindata.ESN;
            resdata.MaintainName = maindata.MaintainName;
            resdata.Period = Surface.MaintainPeriod()[maindata.Period];
            resdata.LastMaintainDate = maindata.lastMaintainDate?.ToString("yyyy-MM-dd");
            resdata.IsAudited = maindata.AuditResult != null;
            resdata.AuditReason = maindata.AuditReason;
            #endregion

            res.AccessState = ResState.Success;
            res.Datas = resdata;
            return res;
        }
        #endregion

        #region 定期保養單 填報
        public JsonResService<string> MaintainManagementApp_Report(MaintainManagementApp_Report datas, string userName)
        {
            #region 變數
            JsonResService<string> res = new JsonResService<string>();
            #endregion

            #region 檢查資料
            var maindata = _db.Equipment_MaintenanceForm
                .Where(x => x.Equipment_MaintenanceFormMember.Any(m => m.Maintainer == userName) 
                && x.EMFSN == datas.EMFSN && (x.Status == "2" || x.Status == "5")).FirstOrDefault();
            if (maindata == null)
                throw new MyCusResException("查無此設備保養單");
            if (string.IsNullOrEmpty(datas.ReportContent.Trim()))
                throw new MyCusResException("請填寫保養填報內容!");
            #endregion

            #region 更新資料
            maindata.Status = "3"; // 待審核
            maindata.ReportTime = DateTime.Now;
            maindata.ReportContent = datas.ReportContent;
            maindata.ReportId = userName;

            _db.Equipment_MaintenanceForm.AddOrUpdate(maindata);
            _db.SaveChanges();
            #endregion

            res.AccessState = ResState.Success;
            res.Datas = "填報成功!";
            return res;
        }
        #endregion

        // 排程
        #region 定期保養單 新增排程
        public JsonResService<string> MaintainForm_CreatingSchedule()
        {
            #region 變數
            JsonResService<string> res = new JsonResService<string>();
            int count = 0;
            #endregion
            try
            {
                #region 查資料
                var maindata = _db.Equipment_MaintainItemValue.AsNoTracking().ToList().Where(x => CheckIfCreateMaintainForm(x));
                #endregion

                #region 新增保養單
                foreach (var item in maindata)
                {
                    CreateMaintainForm(item);
                    count++;
                }
                #endregion

                res.AccessState = ResState.Success;
                res.Datas = $"產生{count}筆保養單!";
                return res;
            }
            catch (Exception ex)
            {
                res.AccessState = ResState.Failed;
                res.Datas = $"產生{count}筆保養單!";
                res.ErrorMessage = "系統異常!";
                return res;
            }
        }
        #endregion

        // Functions
        #region GetMyNameByUserNameOrEmpty
        private string GetMyNameByUserNameOrEmpty(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return "-";
            else
                return _db.AspNetUsers.Where(x => x.UserName == userName).First().MyName;
        }
        #endregion

        #region 是否產生保養單
        public bool CheckIfCreateMaintainForm(Equipment_MaintainItemValue data, bool isNew = false)
        {
            if (!data.IsCreateForm) // 未產單
                if (data.NextMaintainDate <= DateTime.Today.AddMonths(1)) // 下次保養日在一個月內
                    if (isNew || data.EquipmentInfo.EState == "1") // 狀態為正常(非"報修中"或"停用")
                            return true;
            return false;
        }
        #endregion

        #region 新增單一保養單
        public void CreateMaintainForm(Equipment_MaintainItemValue data)
        {
            data.lastMaintainDate = ToAD(data.lastMaintainDate);
            data.NextMaintainDate = ToAD(data.NextMaintainDate);

            string nextMaintainDate = data.NextMaintainDate?.ToString("yyMMdd");
            var lastsn = _db.Equipment_MaintenanceForm.OrderByDescending(x => x.EMFSN).Select(x => x.EMFSN)
                .Where(x => x.StartsWith("M" + nextMaintainDate)).FirstOrDefault();
            var newsn = ComFunc.CreateNextID($"M{nextMaintainDate}" + "%{6}", lastsn);

            // 新增設備保養單
            Equipment_MaintenanceForm newform = new Equipment_MaintenanceForm();
            newform.EMFSN = newsn;
            newform.ESN = data.ESN;
            newform.MISSN = data.MISSN;
            newform.MaintainName = data.Template_MaintainItemSetting.MaintainName;
            newform.Period = data.Period;
            newform.lastMaintainDate = data.lastMaintainDate;
            newform.NextMaintainDate = data.NextMaintainDate.Value;
            newform.Status = "1"; // 待派工
            _db.Equipment_MaintenanceForm.Add(newform);

            // 編輯設備保養項目填值
            data.IsCreateForm = true;
            _db.Equipment_MaintainItemValue.AddOrUpdate(data);

            _db.SaveChanges();
        }
        #endregion

        #region 將DateTime.Year<=1500的年份轉為西元年
        public DateTime? ToAD(DateTime? time)
        {
            if (time.HasValue)
            {
                DateTime result = time.Value;
                int year = result.Year;
                if (year <= 1500) // 年分小於 1500 視為民國
                    year += 1911;

                return new DateTime(year, result.Month, result.Day, result.Hour, result.Minute, result.Second);
            }
            else return null;
        }
        #endregion

        #region 更新設備保養單資訊
        public void UpdateMaintainForm(Equipment_MaintenanceForm data)
        {
            if (_db.Equipment_MaintenanceForm?.Any(x => x.EMFSN == data.EMFSN) != true)
                throw new MyCusResException("查無資料！");

            _db.Equipment_MaintenanceForm.AddOrUpdate(data);
        }
        #endregion

        #region 查詢符合Dto的設備保養單資訊
        /// <summary>
        /// 依據條件查詢Equipment_MaintenanceForm後，以指定資料型別回傳
        /// </summary>
        /// <typeparam name="T">回傳之資料型別</typeparam>
        /// <param name="filter">查詢條件</param>
        /// <returns>The IQueryable of <see cref="T"/></returns>
        public IQueryable<T> GetMaintenanceFormQueryByDto<T>(Expression<Func<Equipment_MaintenanceForm, bool>> filter = null)
            where T : new()
        {
            // 基本查詢
            var query = _db.Equipment_MaintenanceForm.AsQueryable();

            // 如果有過濾條件，應用過濾
            if (filter != null)
                query = query.Where(filter);


            // 映射到目標類型
            if (typeof(T) == typeof(Equipment_MaintenanceForm))
                return (IQueryable<T>)query;
            else
                return query.Select(Helper.MapDatabaseToQuery<Equipment_MaintenanceForm, T>());
        }
        #endregion

        // 藍芽位置整理(借放)
        #region 藍芽位置整理(借放)
        public void BluetoothLocationJson()
        {
            #region 變數
            JsonResService<string> res = new JsonResService<string>();
            string folderPath = @"C:\Users\User\Downloads\BeaconPoint";
            string outputFilePath = @"C:\Users\User\Desktop\bt.csv";

            // 讀取GUID的csv或excel路徑
            string readFilePath = @"C:\Users\User\Downloads\藍芽發射器ELEMENT ID列表.xlsx";
            #endregion

            #region json to csv
            //if (!Directory.Exists(folderPath))
            //{
            //    throw new DirectoryNotFoundException($"資料夾不存在: {folderPath}");
            //}
            //var jsonFiles = Directory.GetFiles(folderPath, "*.json");
            //foreach (var file in jsonFiles)
            //{
            //    // 讀取文件內容
            //    var jsonContent = File.ReadAllText(file);

            //    // 反序列化 JSON 為物件
            //    var data = JsonConvert.DeserializeObject<BluetoothJson>(jsonContent);
            //    if (data != null)
            //    {
            //        foreach (var item in data.pins)
            //        {
            //            //var maindata = _
            //            using (StreamWriter writer = new StreamWriter(outputFilePath, true))
            //            {
            //                var position = item.position == null ? "" : string.Join(",", item.position.Select(n => n.ToString()));
            //                writer.Write($"{data.FSN},{item.GUID},{item.DBID},{item.ElementID},{item.DeviceName},{position}\n");
            //            }
            //        }
            //    }
            //}
            #endregion

            //讀入資料
            var rows = MiniExcel.Query<DBIDData>(readFilePath);
            foreach (var row in rows)
            {
                //var maindata = _db.
            }
        }
        public class DBIDData
        {
            public string GUID { get; set; }
            public string DBID { get; set; }
        }
        #endregion
    }
}