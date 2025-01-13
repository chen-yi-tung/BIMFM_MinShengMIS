using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace MinSheng_MIS.Services
{
    public class SampleSchedule_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;

        public SampleSchedule_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        #region 新增每日巡檢時程安排模板
        public async Task<string> CreateInspectionSampleAsync(IInspectionSampleInfoModifiable data)
        {
            // 資料驗證
            InspectionSampleDataAnnotation(data);

            // 前一筆資料
            var latest = await _db.DailyInspectionSample.OrderByDescending(x => x.DailyTemplateSN).FirstOrDefaultAsync();

            // 建立 InspectionPathSample
            var sample = new DailyInspectionSample
            {
                DailyTemplateSN = ComFunc.GenerateUniqueSn("!{yyMMdd}%{3}", 9, latest.DailyTemplateSN),
                TemplateName = data.TemplateName,
            };
            _db.DailyInspectionSample.Add(sample);

            return sample.DailyTemplateSN;
        }
        #endregion

        #region 建立每日巡檢時程安排模板內容
        public void CreateInspectionSampleContent(IInspectionSampleContentModifiableList data)
        {
            // 資料驗證
            InspectionSampleContentDataAnnotation(data);

            // 批次建立 DailyInspectionSampleContent
            AddRangeSampleContent(data);
        }
        #endregion

        #region 獲取每日巡檢時程安排模板
        public async Task<T> GetInspectionSampleAsync<T>(string dailyTemplateSN) where T : class, new()
        {
            var sample = await _db.DailyInspectionSample.FindAsync(dailyTemplateSN)
                ?? throw new MyCusResException("查無資料！");

            return sample.ToDto<DailyInspectionSample, T>();
        }
        #endregion

        #region 獲取每日巡檢時程安排模板內容
        public List<ISampleScheduleContentDetail> GetSampleScheduleContentList(string dailyTemplateSN)
        {
            var result = _db.DailyInspectionSampleContent.Where(x => x.DailyTemplateSN == dailyTemplateSN)
                .AsEnumerable()
                .Select(x => new SampleScheduleContentDetailModel
                {
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    PathName = x.InspectionPathSample.PathName,
                    Frequency = $"每{x.InspectionPathSample.Frequency}小時",
                    EquipmentCount = x.InspectionPathSample.InspectionDefaultOrder?.Count.ToString() ?? "0",
                    PlanPathSN = x.PlanPathSN
                });

            return result.Cast<ISampleScheduleContentDetail>().ToList();
        }
        #endregion

        #region 刪除每日巡檢時程安排模板
        public void DeleteInspectionSample(DailyInspectionSample data)
        {
            DeleteSampleScheduleContents(data.DailyInspectionSampleContent);

            _db.DailyInspectionSample.Remove(data);
        }
        #endregion

        #region 刪除每日巡檢時程安排模板內容
        public void DeleteSampleScheduleContents(IEnumerable<DailyInspectionSampleContent> data)
        {
            if (data?.Any() != true)
                return;

            _db.DailyInspectionSampleContent.RemoveRange(data);
        }
        #endregion

        //-----資料驗證

        #region DailyInspectionSample 資料驗證
        private void InspectionSampleDataAnnotation(IInspectionSampleInfoModifiable data, string dailyTemplateSN = null)
        {
            // 不可重複：巡檢模板名稱
            var sample = string.IsNullOrEmpty(dailyTemplateSN) ?
                _db.DailyInspectionSample :
                _db.DailyInspectionSample.Where(x => x.DailyTemplateSN != dailyTemplateSN);

            if (sample.Select(x => x.TemplateName).AsEnumerable().Contains(data.TemplateName))
                throw new MyCusResException("巡檢模板名稱已被使用！");
        }
        #endregion

        #region DailyInspectionSampleContent 資料驗證
        internal void InspectionSampleContentDataAnnotation(IInspectionSampleContentModifiableList data)
        {
            if (data.Contents?.Any() != true)
                throw new MyCusResException($"請新增至少一項巡檢路線！");
            // 長度限制
            if (data.Contents.Count() >= 100000)
                throw new MyCusResException($"巡檢路線不可超過100000項！");
            // 關聯性PK是否存在：巡檢路線編號
            if (Helper.AreListsEqualIgnoreOrder(
                data.Contents.Select(x => x.PlanPathSN),
                _db.InspectionPathSample.Select(x => x.PlanPathSN)))
                throw new MyCusResException("巡檢路線不存在！");
        }
        #endregion

        //-----其他
        #region 批次新增 DailyInspectionSampleContent
        /// <summary>
        /// 批次新增每日巡檢時程安排模板內容
        /// </summary>
        /// <param name="data">包含巡檢時程內容列表及每日模板編號(DailyTemplateSN)</param>
        /// <returns>無回傳</returns>
        private void AddRangeSampleContent(IInspectionSampleContentModifiableList data)
        {
            if (data.Contents?.Any() != true)
                return;

            // 不Fetch最後一個Id，每次變更都全部刪除重新建立
            string latestId = null;

            _db.DailyInspectionSampleContent.AddRange(Helper.AddOrUpdateList(
                data.Contents, // list
                data.DailyTemplateSN, // sn
                latestId, // initialLatestId
                "${SN}%{5}", // format
                14, // emptySnLength
                new string[] { data.DailyTemplateSN }, // param
                (format, Len, lastId, sn) => ComFunc.GenerateUniqueSn(format, Len, lastId, sn),
                (item, param, newId) => new DailyInspectionSampleContent
                {
                    ScheduleSN = newId,
                    DailyTemplateSN = param[0],
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,
                    PlanPathSN = item.PlanPathSN
                }
            ));
        }
        #endregion
    }
}