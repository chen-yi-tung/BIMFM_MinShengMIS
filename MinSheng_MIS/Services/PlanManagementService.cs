using Microsoft.Ajax.Utilities;
using MinSheng_MIS.Attributes;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static MinSheng_MIS.Services.UniParams;

namespace MinSheng_MIS.Services
{
    public class PlanManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly SampleSchedule_ManagementService _sampleScheduleService;

        public PlanManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _sampleScheduleService = new SampleSchedule_ManagementService(db);
        }

        #region 新增工單
        public async Task<string> CreateInspectionPlanAsync(IInspectionPlanInfo data)
        {
            // 無需要驗證的資料

            // 前一筆資料
            var latest = await _db.InspectionPlan.OrderByDescending(x => x.IPSN).FirstOrDefaultAsync();

            // 建立 InspectionPlan
            var plan = new InspectionPlan
            {
                IPSN = ComFunc.GenerateUniqueSn("P!{yyMMdd}%{2}", 9, latest?.IPSN),
                IPName = data.IPName,
                PlanDate = data.PlanDate,
                PlanState = ((int)InspectionPlanState.ToDo).ToString(),
                PlanCreateUserID = HttpContext.Current.User.Identity.Name,
                CreateTime = DateTime.Now,
            };
            _db.InspectionPlan.Add(plan);

            return plan.IPSN;
        }
        #endregion

        #region 建立巡檢時段及執行人員
        public void CreateInspectionPlanContent(IInspectionPlanTimeModifiableList data)
        {
            // 資料驗證
            InspectionPlanTimeDataAnnotation(data);

            // 巡檢路線資訊
            data.Inspections.ForEach(x =>
            {
                var temp = SamplePath_ManagementService.GetSamplePath<InspectionPathSample>(_db, x.PlanPathSN);
                x.PathName = temp.PathName;
                x.Frequency = temp.Frequency;
                x.EquipmentCount = temp.DailyInspectionSampleContent.Count;
            });

            // 批次建立 InspectionPlan_Time 及 InspectionPlan_Member
            AddRangeInspectionPlanTime(data);
        }
        #endregion

        #region 獲取工單
        public async Task<T> GetInspectionPlanAsync<T>(string ipsn) where T : class, new()
        {
            var plan = await _db.InspectionPlan.FindAsync(ipsn)
                ?? throw new MyCusResException("查無資料！");

            T dest = plan.ToDto<InspectionPlan, T>();

            if (dest is IInspectionPlanDetailViewModel info)
            {
                info.PlanDate = plan.PlanDate.ToString("yyyy-MM-dd");
                info.PlanState = ConvertStringToEnum<InspectionPlanState>(plan.PlanState).GetLabel()
                ?? "undefined";

                dest = (T)info;
            }

            return dest;
        }
        #endregion

        #region 獲取巡檢路線及時段
        public List<IInspectionPlanContentDetail> GetInspectionPlanTime(string IPSN)
        {
            var result = _db.InspectionPlan_Time.Where(x => x.IPSN == IPSN)
                .AsEnumerable()
                .Select(x => 
                {
                    var temp = x.ToDto<InspectionPlan_Time, InspectionPlanContentDetail>();
                    temp.InspectionState = ConvertStringToEnum<InspectionState>(x.InspectionState).GetLabel() ?? "undefined";
                    temp.Frequency = $"每{x.Frequency}小時";
                    temp.EquipmentCount = (x.InspectionPlan_Equipment?.Any() == true ?
                        x.InspectionPlan_Equipment.Count.ToString() :
                        x.InspectionPathSample?.InspectionDefaultOrder?.Count.ToString())
                        ?? "0";
                    return temp;
                });

            return result.Cast<IInspectionPlanContentDetail>().ToList();
        }
        #endregion

        #region 獲取巡檢路線設備
        public async Task<List<IInspectionPlanEquipment>> GetInspectionPlanEquipmentAsync(string IPTSN)
        {
            var userDic = await _db.AspNetUsers.ToDictionaryAsync(k => k.UserName, v => v.MyName);
            var test = _db.InspectionPlan_Equipment.Where(x => x.IPTSN == IPTSN).ToList();
            var result = _db.InspectionPlan_Equipment.Where(x => x.IPTSN == IPTSN)
                .AsEnumerable()
                .Select(x => 
                {
                    var temp = x.ToDto<InspectionPlan_Equipment, InspectionPlanEquipment>();
                    temp.EName = x.EquipmentInfo.EName;
                    temp.NO = x.EquipmentInfo.NO;
                    temp.Location = $"{x.EquipmentInfo.Floor_Info.AreaInfo.Area} {x.EquipmentInfo.Floor_Info.FloorName}";
                    temp.ReportUserName = x.ReportUserName != null ? userDic.TryGetValue(x.ReportUserName, out string name) ? name : "undefined" : "-";
                    temp.FillinTime = x.FillinTime?.ToString("yyyy/MM/dd HH:mm");
                    return temp;
                });

            return result.Cast<IInspectionPlanEquipment>().ToList();
        }
        #endregion

        #region 獲取巡檢執行人員
        public List<string> GetInspectionPlanExecutors(string IPTSN)
        {
            var Excutors = _db.InspectionPlan_Member
                .Where(x => x.IPTSN == IPTSN)
                .Select(x => x.UserID)
                .ToList();
            return Excutors;
        }
        #endregion

        #region 獲取巡檢路線設備檢查項目
        public List<IInspectionPlanCheckItem> GetInspectionPlanCheckItems(string IPESN)
        {
            var result = _db.InspectionPlan_EquipmentCheckItem.Where(x => x.IPESN == IPESN)
                .AsEnumerable()
                .Select(x => new InspectionPlanCheckItem
                {
                    Item = x.CheckItemName,
                    Result = x.CheckResult != null ? ConvertStringToEnum<CheckResult>(x.CheckResult).GetLabel() : null,
                });

            return result.Cast<IInspectionPlanCheckItem>().ToList();
        }
        #endregion

        #region 獲取巡檢路線設備填報項目
        public List<IInspectionPlanRportItem> GetInspectionPlanRportItems(string IPESN)
        {
            var result = _db.InspectionPlan_EquipmentReportingItem?.Where(x => x.IPESN == IPESN)
                .AsEnumerable()
                .Select(x => new InspectionPlanRportItem
                {
                    Item = x.ReportValue,
                    Value = x.ReportContent,
                    Unit = x.Unit,
                })
                ?? Enumerable.Empty<object>();

            return result.Cast<IInspectionPlanRportItem>().ToList();
        }
        #endregion

        #region 編輯工單
        public async Task EditInspectionPlanAsync(IUpdateInspectionPlan data)
        {
            var plan = await _db.InspectionPlan
                .SingleOrDefaultAsync(x => x.IPSN == data.IPSN)
                ?? throw new MyCusResException("工單不存在！");

            if (!IsEnumEqualToStr(plan.PlanState, InspectionPlanState.ToDo))
                throw new MyCusResException("工單已開始執行，不可編輯！");

            plan.IPName = data.IPName;
            plan.PlanDate = data.PlanDate;

            _db.InspectionPlan.AddOrUpdate(plan);
        }
        #endregion

        #region 編輯巡檢時段及執行人員
        public async Task EditInspectionPlanContentAsync(IInspectionPlanTimeModifiableList data)
        {
            var plan = await _db.InspectionPlan
                .SingleOrDefaultAsync(x => x.IPSN == data.IPSN)
                ?? throw new MyCusResException("工單不存在！");

            // 資料驗證
            InspectionPlanTimeDataAnnotation(data);

            // 清空巡檢時段及執行人員
            DeleteInspectionPlanContent(plan.InspectionPlan_Time);
            await _db.SaveChangesAsync();

            // 建立巡檢時段及執行人員
            CreateInspectionPlanContent(data);
        }
        #endregion

        #region 刪除工單
        public void DeleteInspectionPlan(InspectionPlan data)
        {
            if (!IsEnumEqualToStr(data.PlanState, InspectionPlanState.ToDo))
                throw new MyCusResException("工單已開始執行，不可刪除！");

            // 刪除巡檢時段及執行人員
            DeleteInspectionPlanContent(data.InspectionPlan_Time);

            // 刪除工單
            _db.InspectionPlan.Remove(data);
        }
        #endregion

        #region 刪除巡檢時段及執行人員
        public void DeleteInspectionPlanContent(IEnumerable<InspectionPlan_Time> data)
        {
            if (data?.Any() != true)
                return;

            // 刪除執行人員
            if (data.Any(x => x.InspectionPlan_Member?.Count > 0))
                _db.InspectionPlan_Member.RemoveRange(data.SelectMany(x => x.InspectionPlan_Member));
            // 刪除巡檢時段
            _db.InspectionPlan_Time.RemoveRange(data);
        }
        #endregion

        //-----資料驗證
        #region InspectionPlan_Time 資料驗證
        private void InspectionPlanTimeDataAnnotation(IInspectionPlanTimeModifiableList data)
        {
            data.SetInspectionSampleContent();
            // 巡檢路線資料驗證
            _sampleScheduleService.InspectionSampleContentDataAnnotation(data);
            // 執行人員資料驗證
            foreach (var item in data.Inspections)
                InspectionPlanMemberDataAnnotation(item);
        }
        #endregion

        #region InspectionPlan_Member 資料驗證
        private void InspectionPlanMemberDataAnnotation(IInspectionPlanExecutors data)
        {
            if (data.Executors?.Any() != true)
                throw new MyCusResException($"請新增至少一位執行人員！");
            // 長度限制
            if (data.Executors.Count() >= 100)
                throw new MyCusResException($"執行人員不可超過100位！");
            // 關聯性PK是否存在：人員帳號
            var users = _db.AspNetUsers.Where(x => x.IsEnabled).Select(x => x.UserName);
            if (!data.Executors.All(x => users.Contains(x)))
                throw new MyCusResException("執行人員不存在！");
        }
        #endregion

        //-----其他
        #region 批次新增 InspectionPlan_Time
        /// <summary>
        /// 批次新增巡檢路線及巡檢時間
        /// </summary>
        /// <param name="data">包含巡檢時程內容列表及巡檢計畫編號(IPSN)</param>
        /// <returns>無回傳</returns>
        private void AddRangeInspectionPlanTime(IInspectionPlanTimeModifiableList data)
        {
            if (data.Inspections?.Any() != true)
                return;

            // 不Fetch最後一個Id，每次變更都全部刪除重新建立
            string latestId = null;

            _db.InspectionPlan_Time.AddRange(Helper.AddOrUpdateList(
                data.Inspections, // list
                data.IPSN, // sn
                latestId, // initialLatestId
                "${SN}%{4}", // format
                13, // emptySnLength
                new string[] { data.IPSN }, // param
                (format, Len, lastId, sn) => ComFunc.GenerateUniqueSn(format, Len, lastId, sn),
                (item, param, newTimeId) => new InspectionPlan_Time
                {
                    IPTSN = newTimeId,
                    IPSN = param[0],
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,
                    PlanPathSN = item.PlanPathSN,
                    PathName = item.PathName,
                    InspectionNum = item.EquipmentCount,
                    Frequency = item.Frequency,
                    InspectionState = ((int)InspectionState.ToDo).ToString(),
                    InspectionPlan_Member = Helper.AddOrUpdateList(
                        item.Executors, // list
                        newTimeId, // sn
                        latestId, // initialLatestId
                        "${SN}%{2}", // format
                        15, // emptySnLength
                        new string[] {}, // param
                        (format, Len, lastId, sn) => ComFunc.GenerateUniqueSn(format, Len, lastId, sn),
                        (member, empty, newMemberId) => new InspectionPlan_Member
                        {
                            IPTMSN = newMemberId,
                            IPTSN = newTimeId,
                            UserID = member,
                        }
                    )
                }
            ));
        }
        #endregion
    }
}