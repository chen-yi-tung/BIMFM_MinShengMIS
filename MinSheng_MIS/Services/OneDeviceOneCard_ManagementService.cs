using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace MinSheng_MIS.Services
{
    public class OneDeviceOneCard_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly ComFunc _cFunc;

        public OneDeviceOneCard_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _cFunc = new ComFunc();
        }

        #region 新增一機一卡模板
        public async Task<string> CreateDeviceCardAsync(IUpdateDeviceCard data)
        {
            // 資料驗證
            DeviceCardDataAnnotation(data);

            // 建立 Template_OneDeviceOneCard
            var deviceCard = new Template_OneDeviceOneCard
            {
                TSN = await GenerateTSNAsync(),
                SampleName = data.SampleName,
                Frequency = data.Frequency
            };
            _db.Template_OneDeviceOneCard.Add(deviceCard);

            return deviceCard.TSN;
        }
        #endregion

        #region 批次新增增設基本資料欄位
        public async Task CreateAddFieldListAsync(IUpdateAddFieldList data)
        {
            // 資料驗證
            await AddFieldDataAnnotationAsync(data);

            // 建立 Template_AddField
            await AddRangeAddFieldAsync(data);
        }
        #endregion

        #region 批次新增保養項目設定
        public async Task CreateMaintainItemListAsync(IUpdateMaintainItemList data)
        {
            // 資料驗證
            await MaintainItemDataAnnotationAsync(data);

            // 建立 Template_MaintainItemSetting
            await AddRangeMaintainItemAsync(data);
        }
        #endregion

        #region 批次新增檢查項目
        public async Task CreateCheckItemListAsync(IUpdateCheckItemList data)
        {
            // 資料驗證
            await CheckItemDataAnnotationAsync(data);

            // 建立 Template_CheckItem
            await AddRangeCheckItemAsync(data);
        }
        #endregion

        #region 批次新增填報項目
        public async Task CreateReportItemListAsync(IUpdateReportItemList data)
        {
            // 資料驗證
            await ReportItemDataAnnotationAsync(data);

            // 建立 Template_CheckItem
            await AddRangeReportItemAsync(data);
        }
        #endregion

        //-----資料驗證
        #region Template_OneDeviceOneCard資料驗證
        private void DeviceCardDataAnnotation(IUpdateDeviceCard data)
        {
            // 不可重複：模板
            if (_db.Template_OneDeviceOneCard.Select(x => x.SampleName).ToList().Contains(data.SampleName))
                throw new MyCusResException("模板名稱已被使用!");
        }
        #endregion

        #region AddField資料驗證
        private async Task AddFieldDataAnnotationAsync(IUpdateAddFieldList data)
        {
            // 驗證新增設基本欄位
            ValidateList(data.AFNameList, "增設基本欄位", 100);
            // 與既有欄位進行比對
            if (await _db.Template_AddField.Where(x => x.TSN == data.TSN).AnyAsync(x => data.AFNameList.Contains(x.FieldName)))
                throw new MyCusResException("增設基本欄位不可重複!");
        }
        #endregion

        #region MaintainItem資料驗證
        private async Task MaintainItemDataAnnotationAsync(IUpdateMaintainItemList data)
        {
            // 驗證新增設基本欄位
            ValidateList(data.MINameList, "保養項目", 100);
            // 與既有欄位進行比對
            if (await _db.Template_MaintainItemSetting.Where(x => x.TSN == data.TSN).AnyAsync(x => data.MINameList.Contains(x.MaintainName)))
                throw new MyCusResException("保養項目名稱不可重複!");
        }
        #endregion

        #region CheckItem資料驗證
        private async Task CheckItemDataAnnotationAsync(IUpdateCheckItemList data)
        {
            // 當檢查項目不為空，則檢查頻率為必填
            if (data.Frequency == null)
                throw new MyCusResException("請填寫檢查頻率!");
            // 驗證新增設基本欄位
            ValidateList(data.CINameList, "檢查項目", 100);
            // 與既有欄位進行比對
            if (await _db.Template_CheckItem.Where(x => x.TSN == data.TSN).AnyAsync(x => data.CINameList.Contains(x.CheckItemName)))
                throw new MyCusResException("檢查項目名稱不可重複!");
        }
        #endregion

        #region ReportItem資料驗證
        private async Task ReportItemDataAnnotationAsync(IUpdateReportItemList data)
        {
            // 當填報項目不為空，則檢查頻率為必填
            if (data.Frequency == null)
                throw new MyCusResException("請填寫檢查頻率!");
            ValidateList(data.RIList.Select(x => x.RIName), "填報項目", 100);
            // 與既有欄位進行比對
            if (await _db.Template_ReportingItem.Where(x => x.TSN == data.TSN).AnyAsync(x => data.RIList.Select(r => r.RIName).Contains(x.ReportingItemName)))
                throw new MyCusResException("檢查項目名稱不可重複!");
            //// List長度限制
            //if (data.RIList.Count() > 100)
            //    throw new MyCusResException("填報項目不可超過100項!");
            //// 不可重複：填報項目名稱
            //if (ListItemDuplicated(data.RIList.Select(x => x.RIName)))
            //    throw new MyCusResException("填報項目名稱不可重複!");
        }
        #endregion

        //-----其他
        #region 產生一機一卡模板唯一編碼
        /// <summary>
        /// 產生一機一卡模板唯一編碼
        /// </summary>
        /// <returns>唯一編碼</returns>
        public async Task<string> GenerateTSNAsync()
        {
            string format = "!{yyMMdd}%{2}";
            string emptySN = new string('0', 8); // 產生11位數的'0'

            // 前一筆資料
            var latest = await _db.Template_OneDeviceOneCard.OrderByDescending(x => x.TSN).FirstOrDefaultAsync();
            // SN碼
            return latest == null ?
                _cFunc.CreateNextID(format, emptySN) :
                _cFunc.CreateNextID(format, latest.TSN);
        }
        #endregion

        #region 獲取最後一個編碼
        public async Task<string> GetLatestIdWithSameTsnAsync(string tsn, IQueryable<dynamic> query, string fieldName)
        {
            return await query.Where($"TSN == @0", tsn)
                .OrderBy($"{fieldName} descending")
                .FirstOrDefaultAsync();
        }
        #endregion

        #region 產生唯一編碼
        /// <summary>
        /// 產生唯一編碼
        /// </summary>
        /// <param name="tsn">TSN碼</param>
        /// <param name="latestId">前一筆資料唯一編碼</param>
        /// <param name="fieldName">唯一編碼欄位名稱</param>
        /// <returns>唯一編碼</returns>
        /// <remarks>唯一編碼規則為"${TSN}%{3}"</remarks>
        public string GenerateUniqueIdByTSN(string tsn, string latestId, string fieldName)
        {
            string format = tsn + "%{3}";
            string emptySN = new string('0', 11); // 產生11位數的'0'

            // SN碼
            return latestId == null ?
                _cFunc.CreateNextID(format, emptySN) :
                _cFunc.CreateNextID(format, latestId);
        }
        #endregion

        #region 批次新增 Template_AddField
        /// <summary>
        /// 批次新增設備一機一卡基本資料欄位
        /// </summary>
        /// <param name="data">包含一機一卡基本資料欄位名稱列表及一機一卡模板編碼(TSN)</param>
        /// <returns>無回傳</returns>
        private async Task AddRangeAddFieldAsync(IUpdateAddFieldList data)
        {
            // Fetch最後一個Id
            string latestId = await GetLatestIdWithSameTsnAsync(data.TSN, _db.Template_AddField, "AFSN");

            _db.Template_AddField.AddRange(Helper.AddOrUpdateList(
                data.AFNameList,
                data.TSN,
                latestId,
                (esn, lastId) => GenerateUniqueIdByTSN(data.TSN, lastId, "AFSN"),
                (x, e, lastId, newId) => new Template_AddField
                {
                    AFSN = newId,  // 使用新生成的Id
                    TSN = e,
                    FieldName = x
                }
            ));
        }
        #endregion

        #region 批次新增 Template_MaintainItemSetting
        /// <summary>
        /// 批次新增設備一機一卡保養項目設定
        /// </summary>
        /// <param name="data">包含一機一卡保養項目名稱列表及一機一卡模板編碼(TSN)</param>
        /// <returns>無回傳</returns>
        private async Task AddRangeMaintainItemAsync(IUpdateMaintainItemList data)
        {
            // Fetch最後一個Id
            string latestId = await GetLatestIdWithSameTsnAsync(data.TSN, _db.Template_MaintainItemSetting, "MISSN");

            _db.Template_MaintainItemSetting.AddRange(Helper.AddOrUpdateList(
                data.MINameList,
                data.TSN,
                latestId,
                (esn, lastId) => GenerateUniqueIdByTSN(data.TSN, lastId, "MISSN"),
                (x, e, lastId, newId) => new Template_MaintainItemSetting
                {
                    MISSN = newId,  // 使用新生成的Id
                    TSN = e,
                    MaintainName = x
                }
            ));
        }
        #endregion

        #region 批次新增 Template_CheckItem 
        /// <summary>
        /// 批次新增設備一機一卡檢查項目
        /// </summary>
        /// <param name="data">包含一機一卡檢查項目名稱列表及一機一卡模板編碼(TSN)</param>
        /// <returns>無回傳</returns>
        private async Task AddRangeCheckItemAsync(IUpdateCheckItemList data)
        {
            // 只調用一次 GetLatestIdWithSameTsnAsync
            string latestId = await GetLatestIdWithSameTsnAsync(data.TSN, _db.Template_CheckItem, "CISN");

            _db.Template_CheckItem.AddRange(Helper.AddOrUpdateList(
                data.CINameList,
                data.TSN,
                latestId,
                (esn, lastId) => GenerateUniqueIdByTSN(data.TSN, lastId, "CISN"),
                (x, e, lastId, newId) => new Template_CheckItem
                {
                    CISN = newId,  // 使用新生成的Id
                    TSN = e,
                    CheckItemName = x
                }
            ));
        }
        #endregion

        #region 批次新增 Template_ReportingItem 
        /// <summary>
        /// 批次新增設備一機一卡填報項目
        /// </summary>
        /// <param name="data">包含一機一卡填報項目列表(包含名稱及單位)及一機一卡模板編碼(TSN)</param>
        /// <returns>無回傳</returns>
        private async Task AddRangeReportItemAsync(IUpdateReportItemList data)
        {
            // Fetch最後一個Id
            string latestId = await GetLatestIdWithSameTsnAsync(data.TSN, _db.Template_ReportingItem, "RISN");

            _db.Template_ReportingItem.AddRange(Helper.AddOrUpdateList(
                data.RIList,
                data.TSN,
                latestId,
                (esn, lastId) => GenerateUniqueIdByTSN(data.TSN, lastId, "RISN"),
                (x, e, lastId, newId) => new Template_ReportingItem
                {
                    RISN = newId,  // 使用新生成的Id
                    TSN = e,
                    ReportingItemName = x.RIName,
                    Unit = x.Unit
                }
            ));
        }
        #endregion

        #region Helper
        private void ValidateList(IEnumerable<string> list, string fieldName, uint listMaxLength)
        {
            if (list != null && list.Any())
            {
                // List長度限制
                if (list.Count() > listMaxLength)
                    throw new MyCusResException($"{fieldName}不可超過{listMaxLength}項!");
                // 不可重複：名稱
                if (ListItemDuplicated(list))
                    throw new MyCusResException($"{fieldName}名稱不可重複!");
            }
        }

        /// <summary>
        /// 不可重複
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool ListItemDuplicated(IEnumerable<string> list)
        {
            return list.Count() != list.Distinct().Count();
        }

        /// <summary>
        /// 檢查List中各item長度
        /// </summary>
        /// <param name="list">來源List</param>
        /// <param name="max">長度上限(僅接受正整數，可null)</param>
        /// <param name="min">長度下限(僅接受正整數，預設0)</param>
        /// <returns></returns>
        private bool ListItemLength(IEnumerable<string> list, uint? max, uint min = 0)
        {
            // 如果 max 是 null，表示沒有上限，只檢查下限
            return list.All(item => item.Length >= min && (!max.HasValue || item.Length <= max.Value));
        }
        #endregion
    }
}