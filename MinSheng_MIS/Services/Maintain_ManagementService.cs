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

namespace MinSheng_MIS.Services
{
    public class Maintain_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;

        public Maintain_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        #region 定期保養單 詳情
        public JsonResService<Maintain_ManagementDetailViewModel> MaintainManagement_Details(string emfsn)
        {
            #region 變數
            JsonResService<Maintain_ManagementDetailViewModel> res = new JsonResService<Maintain_ManagementDetailViewModel>();
            #endregion

            #region 資料檢查
            var data = _db.Equipment_MaintenanceForm.Find(emfsn);

            if (data == null)
                throw new MyCusResException("查無此設備保養單");
            #endregion

            #region 塞資料
            Maintain_ManagementDetailViewModel datas = new Maintain_ManagementDetailViewModel();
            datas.EMFSN = data.EMFSN;
            datas.Status = Surface.MaintainStatus()[data.Status];
            datas.MaintainName = data.MaintainName;
            datas.Maintainer = string.Join("、", data.Equipment_MaintenanceFormMember.Select(x => x.Maintainer));
            datas.EName = data.EquipmentInfo.EName;
            datas.EState = Surface.EState()[data.EquipmentInfo.EState];
            datas.NO = data.EquipmentInfo.NO;
            datas.Location = $"{data.EquipmentInfo.Floor_Info.FloorName} {data.EquipmentInfo.Floor_Info.AreaInfo.Area}";
            datas.Period = Surface.MaintainPeriod()[data.Period];
            datas.LastMaintainDate = data.lastMaintainDate?.ToString("yyyy/MM/dd") ?? "";
            datas.NextMaintainDate = data.NextMaintainDate.ToString("yyyy/MM/dd");
            datas.ReportId = data.ReportId;
            datas.ReportTime = data.ReportTime?.ToString("yyyy/MM/dd HH:mm") ?? "";
            datas.ReportContent = data.ReportContent;
            datas.AuditResult = data.AuditResult.ToString().ToLower();
            datas.AuditId = data.AuditId;
            datas.AuditTime = data.AuditTime?.ToString("yyyy/MM/dd HH:mm") ?? "";
            datas.AuditReason = data.AuditReason;
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
                var emfsndata = _db.Equipment_MaintenanceForm.Find(emfsn);
                if (emfsndata == null)
                    throw new MyCusResException("查無此定期保養單");

                emfsndata.NextMaintainDate = datas.NextMaintainDate;
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

            

            res.AccessState = ResState.Success;
            res.Datas = "";
            return res;
        }
        #endregion

        // APP
        #region 定期保養單 列表
        public JsonResService<MaintainManagementApp_ListViewModel> MaintainManagementApp_List(string Status)
        {
            #region 變數
            JsonResService<MaintainManagementApp_ListViewModel> res = new JsonResService<MaintainManagementApp_ListViewModel>();
            MaintainManagementApp_ListViewModel resdata = new MaintainManagementApp_ListViewModel();
            List<MaintainManagementApp_ListItem> maintainlist = new List<MaintainManagementApp_ListItem>();
            #endregion

            #region 查資料
            var maindata = _db.Equipment_MaintenanceForm.Where(x => x.Status == "2" || x.Status == "5").AsNoTracking().ToList();
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
                resdataitem.RFIDList = _db.RFID.Where(x => x.ESN == maintainform.ESN).Select(x => x.RFIDExternalCode).AsNoTracking().ToList();
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
            resdata.MaintainFormLsit = maintainlist;
            #endregion

            res.AccessState = ResState.Success;
            res.Datas = resdata;
            return res;
        }
        #endregion

        #region 定期保養單 填報詳情
        public JsonResService<MaintainManagementApp_Detail> MaintainManagementApp_Detail(string EMFSN)
        {
            #region 變數
            JsonResService<MaintainManagementApp_Detail> res = new JsonResService<MaintainManagementApp_Detail>();
            MaintainManagementApp_Detail resdata = new MaintainManagementApp_Detail();
            #endregion

            #region 塞資料
            var maindata = _db.Equipment_MaintenanceForm
                .Where(x => (x.Status == "2" || x.Status == "5") && x.EMFSN == EMFSN).FirstOrDefault();
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
                .Where(x => (x.Status == "2" || x.Status == "5") && x.EMFSN == datas.EMFSN).FirstOrDefault();
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

        #region 定期保養單 填報
        //public JsonResService<string> MaintainManagementApp_Report(MaintainManagementApp_Report datas, string userName)
        //{
        //    #region 變數
        //    JsonResService<string> res = new JsonResService<string>();
        //    #endregion

        //    #region 更新資料
        //    var maindata = _db.Equipment_MaintenanceForm
        //        .Where(x => (x.Status == "2" || x.Status == "5") && x.EMFSN == datas.EMFSN).FirstOrDefault();
        //    if (maindata == null)
        //        throw new MyCusResException("查無此設備保養單");

        //    maindata.Status = "3"; // 待審核
        //    maindata.ReportTime = DateTime.Now;
        //    maindata.ReportContent = datas.ReportContent;
        //    maindata.ReportId = userName;

        //    _db.Equipment_MaintenanceForm.AddOrUpdate(maindata);
        //    _db.SaveChanges();
        //    #endregion

        //    res.AccessState = ResState.Success;
        //    res.Datas = "填報成功!";
        //    return res;
        //}
        #endregion
    }
}