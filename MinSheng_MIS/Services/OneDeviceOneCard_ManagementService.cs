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
        private readonly EquipmentInfo_ManagementService _eMgmtService;
        private readonly SamplePath_ManagementService _pSamplePathService;

        public OneDeviceOneCard_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _eMgmtService = new EquipmentInfo_ManagementService(_db);
            _pSamplePathService = new SamplePath_ManagementService(_db);
        }

        #region 新增一機一卡模板
        public async Task<string> CreateOneDeviceOneCardAsync(IUpdateDeviceCard data)
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
        public async Task CreateAddFieldListAsync(ICreateAddFieldList data)
        {
            // 資料驗證
            await AddFieldDataAnnotationAsync(data);

            // 建立 Template_AddField
            await AddRangeAddFieldAsync(data);
        }
        #endregion

        #region 批次新增保養項目設定
        public async Task CreateMaintainItemListAsync(ICreateMaintainItemList data)
        {
            // 資料驗證
            await MaintainItemDataAnnotationAsync(data);

            // 建立 Template_MaintainItemSetting
            await AddRangeMaintainItemAsync(data);
        }
        #endregion

        #region 批次新增檢查項目
        public async Task CreateCheckItemListAsync(ICreateCheckItemList data)
        {
            // 資料驗證
            await CheckItemDataAnnotationAsync(data);

            // 建立 Template_CheckItem
            await AddRangeCheckItemAsync(data);
        }
        #endregion

        #region 批次新增填報項目
        public async Task CreateReportItemListAsync(ICreateReportItemList data)
        {
            // 資料驗證
            await ReportItemDataAnnotationAsync(data);

            // 建立 Template_CheckItem
            await AddRangeReportItemAsync(data);
        }
        #endregion

        #region 更新增設基本資料欄位 TODO
        #endregion

        #region 獲取一機一卡模板
        public async Task<T> GetOneDeviceOneCardAsync<T>(string TSN) where T : class, new()
        {
            var template = await _db.Template_OneDeviceOneCard.FindAsync(TSN)
                ?? throw new MyCusResException("查無資料!");

            return template.ToDto<Template_OneDeviceOneCard, T>();
        }
        #endregion

        #region 獲取增設基本資料欄位
        public async Task<List<IAddFieldDetail>> GetAddFieldListAsync(string TSN)
        {
            var result = await _db.Template_AddField.Where(x => x.TSN == TSN)
                .Select(x => new AddFieldDetailModel
                {
                    AFSN = x.AFSN,
                    Value = x.FieldName,
                }).ToListAsync();

            return result.Cast<IAddFieldDetail>().ToList();
        }
        #endregion

        #region 獲取保養項目列表
        public async Task<List<IMaintainItemDetail>> GetMaintainItemListAsync(string TSN)
        {
            var result = await _db.Template_MaintainItemSetting.Where(x => x.TSN == TSN)
                .Select(x => new MaintainItemDetailModel
                {
                    MISSN = x.MISSN,
                    Value = x.MaintainName,
                }).ToListAsync();

            return result.Cast<IMaintainItemDetail>().ToList();
        }
        #endregion

        #region 獲取檢查項目列表
        public async Task<List<ICheckItemDetail>> GetCheckItemDetailListAsync(string TSN)
        {
            var result = await _db.Template_CheckItem.Where(x => x.TSN == TSN)
                .Select(x => new CheckItemDetailModel
                {
                    CISN = x.CISN,
                    Value = x.CheckItemName,
                }).ToListAsync();

            return result.Cast<ICheckItemDetail>().ToList();
        }
        #endregion

        #region 獲取填報項目列表
        public async Task<List<IReportItemDetail>> GetReportItemDetailListAsync(string TSN)
        {
            var result = await _db.Template_ReportingItem.Where(x => x.TSN == TSN)
                .Select(x => new ReportItemDetailModel 
                { 
                    RISN = x.RISN, 
                    Value = x.ReportingItemName, 
                    Unit = x.Unit 
                }).ToListAsync();

            return result.Cast<IReportItemDetail>().ToList();
        }
        #endregion

        #region 刪除一機一卡模板 Not Done
        public async Task DeleteOneDeviceOneCardAsync(Template_OneDeviceOneCard card)
        {
            var equipments = card.EquipmentInfo;
            // 刪除模板與設備的關聯
            foreach (var e in equipments)
                await _eMgmtService.UpdateEquipmentInfoAsync(e.ToDto<EquipmentInfo, UpdateEquipmentInfoInstance>());
            // 刪除使用該模板之設備待執行工單 TODO
            // (使用ESN關聯)

            // 刪除使用該模板之巡檢預設順序 TODO
            // (使用ESN->RFID進行關聯)

            // 刪除模板
            _db.Template_OneDeviceOneCard.Remove(card);
        }
        #endregion

        #region 批次刪除增設基本資料欄位
        public void DeleteAddFieldList(IDeleteAddFieldList data)
        {
            if (data?.AFSN.Any() != true) return;

            var fields = _db.Template_AddField
                .Where(x => data.AFSN.Contains(x.AFSN))
                .AsEnumerable();

            // 刪除關聯的 Equipment_AddFieldValue
            _eMgmtService.DeleteAddFieldValueList(
                new DeleteAddFieldValueList(
                    fields.SelectMany(x => x.Equipment_AddFieldValue
                        .Select(e => e.EAFVSN))
                    .AsEnumerable()
                )
            );

            // 刪除 Template_AddField
            _db.Template_AddField.RemoveRange(fields);
        }
        #endregion

        #region 批次刪除保養項目設定
        public void DeleteMaintainItemList(IDeleteMaintainItemList data)
        {
            if (data?.MISSN.Any() != true) return;

            var items = _db.Template_MaintainItemSetting
                .Where(x => data.MISSN.Contains(x.MISSN))
                .AsEnumerable();

            // 刪除相關待派工及待執行的定期保養單
            // 刪除關聯的 Equipment_MaintainItemValue
            _eMgmtService.DeleteMaintainItemValueList(
                new DeleteMaintainItemValueList(
                    items.SelectMany(x => x.Equipment_MaintainItemValue
                        .Select(e => e.EMIVSN))
                    .AsEnumerable()
                )
            );

            // 刪除 Template_MaintainItemSetting
            _db.Template_MaintainItemSetting.RemoveRange(items);
        }
        #endregion

        #region 批次刪除檢查項目
        public void DeleteCheckItemList(IDeleteCheckItemList data)
        {
            if (data?.CISN.Any() != true) return;

            var items = _db.Template_CheckItem
                .Where(x => data.CISN.Contains(x.CISN))
                .AsEnumerable();

            // 刪除 Template_CheckItem
            _db.Template_CheckItem.RemoveRange(items);
        }
        #endregion

        #region 批次刪除填報項目
        public void DeleteReportItemList(IDeleteReportItemList data)
        {
            if (data?.RISN.Any() != true) return;

            var items = _db.Template_ReportingItem
                .Where(x => data.RISN.Contains(x.RISN))
                .AsEnumerable();

            // 刪除 Template_ReportingItem
            _db.Template_ReportingItem.RemoveRange(items);
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
        private async Task AddFieldDataAnnotationAsync(ICreateAddFieldList data)
        {
            // 驗證新增設基本欄位
            ValidateList(data.AFNameList, "增設基本欄位", 100);
            // 與既有欄位進行比對
            if (await _db.Template_AddField.Where(x => x.TSN == data.TSN).AnyAsync(x => data.AFNameList.Contains(x.FieldName)))
                throw new MyCusResException("增設基本欄位不可重複!");
        }
        #endregion

        #region MaintainItem資料驗證
        private async Task MaintainItemDataAnnotationAsync(ICreateMaintainItemList data)
        {
            // 驗證新增設基本欄位
            ValidateList(data.MINameList, "保養項目", 100);
            // 與既有欄位進行比對
            if (await _db.Template_MaintainItemSetting.Where(x => x.TSN == data.TSN).AnyAsync(x => data.MINameList.Contains(x.MaintainName)))
                throw new MyCusResException("保養項目名稱不可重複!");
        }
        #endregion

        #region CheckItem資料驗證
        private async Task CheckItemDataAnnotationAsync(ICreateCheckItemList data)
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
        private async Task ReportItemDataAnnotationAsync(ICreateReportItemList data)
        {
            // 當填報項目不為空，則檢查頻率為必填
            if (data.Frequency == null)
                throw new MyCusResException("請填寫檢查頻率!");
            ValidateList(data.RIList.Select(x => x.RIName), "填報項目", 100);
            // 與既有欄位進行比對
            var riNameList = data.RIList.Select(r => r.RIName).ToList();
            if (await _db.Template_ReportingItem
                .AnyAsync(x => x.TSN == data.TSN && riNameList.Contains(x.ReportingItemName)))
                throw new MyCusResException("檢查項目名稱不可重複!");
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
                ComFunc.CreateNextID(format, emptySN) :
                ComFunc.CreateNextID(format, latest.TSN);
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
        /// 產生 Template_AddField/ Template_MaintainItemSetting/ Template_CheckItem/ Template_ReportingItem 唯一編碼
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
                ComFunc.CreateNextID(format, emptySN) :
                ComFunc.CreateNextID(format, latestId);
        }
        #endregion

        #region 批次新增 Template_AddField
        /// <summary>
        /// 批次新增設備一機一卡基本資料欄位
        /// </summary>
        /// <param name="data">包含一機一卡基本資料欄位名稱列表及一機一卡模板編碼(TSN)</param>
        /// <returns>無回傳</returns>
        private async Task AddRangeAddFieldAsync(ICreateAddFieldList data)
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
        private async Task AddRangeMaintainItemAsync(ICreateMaintainItemList data)
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
        private async Task AddRangeCheckItemAsync(ICreateCheckItemList data)
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
        private async Task AddRangeReportItemAsync(ICreateReportItemList data)
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
                if (list.Count() != list.Distinct().Count())
                    throw new MyCusResException($"{fieldName}名稱不可重複!");
            }
        }
        #endregion
    }
}