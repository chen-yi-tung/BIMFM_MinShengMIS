using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System.Linq.Dynamic.Core;
using System.Data.Entity.Migrations;
using MinSheng_MIS.Surfaces;

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
        //public JsonResService<string> MaintainManagement_Audit(Maintain_ManagementAuditViewModel datas, string userName)
        //{
        //    JsonResService<string> res = new JsonResService<string>();

        //    #region 更新保養單資料、新增保養單派工
        //    foreach (var emfsn in datas.EMFSN)
        //    {
        //        var emfsndata = _db.Equipment_MaintenanceForm.Find(emfsn);
        //        emfsndata.NextMaintainDate = datas.NextMaintainDate;
        //        emfsndata.Status = "2"; // 待執行
        //        emfsndata.Dispatcher = userName;
        //        emfsndata.DispatcherTime = DateTime.Now;
        //        var lastsn = _db.Equipment_MaintenanceFormMember.Where(x => x.EMFSN == emfsn)
        //                .OrderByDescending(x => x.EMFMSN).Select(x => x.EMFMSN).FirstOrDefault();
        //        foreach (var maintainer in datas.Maintainer)
        //        {
        //            lastsn = ComFunc.CreateNextID(emfsn + "%{2}", lastsn);
        //            var emfmsndata = new Equipment_MaintenanceFormMember();
        //            emfmsndata.EMFMSN = lastsn;
        //            emfmsndata.EMFSN = emfsn;
        //            emfmsndata.Maintainer = maintainer;
        //            _db.Equipment_MaintenanceFormMember.Add(emfmsndata);
        //        }
        //    }
        //    _db.SaveChanges();
        //    #endregion

        //    res.AccessState = ResState.Success;
        //    res.Datas = "派工成功!";
        //    return res;
        //}
        #endregion

    }
}