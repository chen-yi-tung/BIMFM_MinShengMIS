using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Data.Entity;
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
        private readonly SamplePath_ManagementService _samplePathService;


        public PlanManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _sampleScheduleService = new SampleSchedule_ManagementService(db);
            _samplePathService = new SamplePath_ManagementService(_db);
        }

        #region 新增工單
        public async Task<string> CreateInspectionPlanAsync(IInspectionPlanModifiable data)
        {
            // 無需要驗證的資料

            // 前一筆資料
            var latest = await _db.InspectionPlan.OrderByDescending(x => x.IPSN).FirstOrDefaultAsync();

            // 建立 InspectionPlan
            var plan = new InspectionPlan
            {
                IPSN = ComFunc.GenerateUniqueSn("P!{yyMMdd}%{2}", 9, latest.IPSN),
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
        public async Task CreateInspectionPlanContentAsync(IInspectionPlanTimeModifiableList data)
        {
            // 資料驗證
            InspectionPlanTimeDataAnnotation(data);
            _sampleScheduleService.InspectionSampleContentDataAnnotation(
                new SampleContentModifiableListInstance 
                { 
                    DailyTemplateSN = null, 
                    Contents = data.Inspections.Cast<InspectionSampleContent>() 
                });

            // 巡檢路線資訊
            var tasks = data.Inspections.Select(async x =>
            {
                var temp = await _samplePathService.GetSamplePathAsync<InspectionPathSample>(x.PlanPathSN);
                x.PathName = temp.PathName;
                x.Frequency = temp.Frequency;
                x.EquipmentCount = temp.DailyInspectionSampleContent.Count;
            });
            await Task.WhenAll(tasks);

            // 批次建立 InspectionPlan_Time 及 InspectionPlan_Member
            AddRangeInspectionPlanTime(data);
        }
        #endregion

        //-----資料驗證
        #region InspectionPlan_Time 資料驗證
        private void InspectionPlanTimeDataAnnotation(IInspectionPlanTimeModifiableList data)
        {
            if (data.Inspections?.Any() != true)
                throw new MyCusResException($"請新增至少一項巡檢路線！");
            // 長度限制
            if (data.Inspections.Count() >= 100000)
                throw new MyCusResException($"巡檢路線不可超過100000項！");
            // 關聯性PK是否存在：巡檢路線編號
            if (Helper.AreListsEqualIgnoreOrder(
                data.Inspections.Select(x => x.PlanPathSN),
                _db.InspectionPathSample.Select(x => x.PlanPathSN)))
                throw new MyCusResException("巡檢路線不存在！");
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
            if (Helper.AreListsEqualIgnoreOrder(
                data.Executors,
                _db.AspNetUsers.Select(x => x.Id)))
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