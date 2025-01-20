using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Web;
using static MinSheng_MIS.Services.UniParams;

namespace MinSheng_MIS.Services
{
    public class OneDeviceOneCard_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly EquipmentInfo_ManagementService _eMgmtService;
        private readonly Maintain_ManagementService _maintainService;

        public OneDeviceOneCard_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _eMgmtService = new EquipmentInfo_ManagementService(_db);
            _maintainService = new Maintain_ManagementService(_db);
        }

        public void SetServer(HttpServerUtilityBase server)
        {
            _eMgmtService.SetServer(server);  // 將 Server 傳遞給服務層
        }

        #region 新增一機一卡模板
        public async Task<string> CreateOneDeviceOneCardAsync(ICreateDeviceCard data)
        {
            // 資料驗證
            DeviceCardDataAnnotation(data);

            // 建立 Template_OneDeviceOneCard
            var deviceCard = new Template_OneDeviceOneCard
            {
                TSN = await GenerateTsnAsync(),
                SampleName = data.SampleName,
                Frequency = data.Frequency
            };
            _db.Template_OneDeviceOneCard.Add(deviceCard);

            return deviceCard.TSN;
        }
        #endregion

        #region 批次新增增設基本資料欄位
        public void CreateAddFieldList(ICreateAddFieldList data)
        {
            // 資料驗證
            AddFieldDataAnnotation(data);

            // 建立 Template_AddField
            AddRangeAddField(data);
        }
        #endregion

        #region 批次新增保養項目設定
        public void CreateMaintainItemList(ICreateMaintainItemList data)
        {
            // 資料驗證
            MaintainItemDataAnnotation(data);

            // 建立 Template_MaintainItemSetting
            AddRangeMaintainItem(data, out _);
        }
        #endregion

        #region 批次新增檢查項目
        public void CreateCheckItemList(ICreateCheckItemList data)
        {
            // 資料驗證
            CheckItemDataAnnotation(data);

            // 建立 Template_CheckItem
            AddRangeCheckItem(data);
        }
        #endregion

        #region 批次新增填報項目
        public void CreateReportItemList(ICreateReportItemList data)
        {
            // 資料驗證
            ReportItemDataAnnotation(data);

            // 建立 Template_CheckItem
            AddRangeReportItem(data);
        }
        #endregion

        #region 更新一機一卡模板
        public async Task UpdateOneDeviceOneCardAsync(IUpdateDeviceCard data)
        {
            // 是否模板存在
            var card = await _db.Template_OneDeviceOneCard.SingleOrDefaultAsync(x => x.TSN == data.TSN)
                ?? throw new MyCusResException("模板不存在！");

            // 資料驗證
            DeviceCardDataAnnotation(data, data.TSN);

            // 更新 Template_OneDeviceOneCard
            var update = data.ToDto<IUpdateDeviceCard, Template_OneDeviceOneCard>();

            // 若巡檢頻率變更，需刪除使用該設備RFID的InspectionPathSample
            if (update.Frequency != card.Frequency)
            {
                // 應刪除的巡檢順序
                var delOrder = card.EquipmentInfo
                    .SelectMany(x => x.RFID)
                    .SelectMany(x => x.InspectionDefaultOrder);
                // 刪除RFID相關巡檢路線
                await _eMgmtService.DeleteRFIDInspectionOrderAsync(delOrder);
            }

            _db.Template_OneDeviceOneCard.AddOrUpdate(update);
        }
        #endregion

        #region 更新增設基本資料欄位
        public async Task UpdateAddFieldListAsync(IUpdateAddFieldList data)
        {
            // 資料驗證
            data.SetAddFieldModifiableList();
            AddFieldDataAnnotation(data);

            // 刪除 Template_AddField
            var afsnList = data.AddItemList?.Select(x => x.AFSN).ToList() ?? new List<string>();
            DeleteAddFieldList(_db.Template_AddField
                    .Where(x => x.TSN == data.TSN && !afsnList.Contains(x.AFSN)));

            // 更新 Template_AddField
            UpdateRangeAddField(data);

            await _db.SaveChangesAsync();

            // 建立 Template_AddField
            data.SetAddFieldModifiableList(true);
            AddRangeAddField(data);
        }
        #endregion

        #region 更新保養項目設定
        /// <summary>
        /// 更新保養項目設定及相關資料表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>
        /// 方法中有變更資料庫，需使用transaction進行roll back
        /// </remarks>
        public async Task UpdateMaintainItemListAsync(IUpdateMaintainItemList data)
        {
            // 資料驗證
            data.SetMaintainItemModifiableList();
            MaintainItemDataAnnotation(data);

            #region 新增/編輯/刪除保養項目
            // 刪除 Template_MaintainItemSetting
            var missnList = data.MaintainItemList?.Select(x => x.MISSN).ToList() ?? new List<string>();
            DeleteMaintainItemList(_db.Template_MaintainItemSetting
                    .Where(x => x.TSN == data.TSN && !missnList.Contains(x.MISSN)));

            // 更新 Template_MaintainItemSetting
            UpdateRangeMaintainItemList(data);

            await _db.SaveChangesAsync();

            // 建立 Template_MaintainItemSetting
            data.SetMaintainItemModifiableList(true);
            AddRangeMaintainItem(data, out var newItems);
            #endregion

            #region AddEquipmentUsedMaintainItem 新增之保養項目於各設備值
            // 具新增之保養項目
            if (data.AddMaintainItemList?.Any() == true)
            {
                await _db.SaveChangesAsync();

                // 建立 Equipment_MaintainItemValue 及30天內的保養單
                if (newItems?.Any() == true)
                {
                    var valueTargetList = data.AddMaintainItemList?.GroupBy(x => x.ESN).Select(x =>
                        new UpdateMaintainItemValueInstance
                        {
                            ESN = x.Key,
                            TSN = data.TSN,
                            MaintainItemList = x.Select(i => new MaintainItemValueModel
                            {
                                MISSN = newItems.SingleOrDefault(n => n.MaintainName == i.MaintainName).MISSN,
                                Period = i.Period,
                                NextMaintainDate = i.NextMaintainDate,
                            }).ToList()
                        })
                        ?? Enumerable.Empty<UpdateMaintainItemValueInstance>();
                    foreach (var value in valueTargetList)
                        await _eMgmtService.CreateEquipmentMaintainItemsValue(value, true);
                }
            }
            #endregion

        }
        #endregion

        #region 更新檢查項目
        public async Task UpdateCheckItemListAsync(IUpdateCheckItemList data)
        {
            // 資料驗證
            data.SetCheckItemModifiableList();
            CheckItemDataAnnotation(data);

            // 刪除 Template_CheckItem
            var cisnList = data.CheckItemList?.Select(x => x.CISN).ToList() ?? new List<string>();
            var delItems = _db.Template_CheckItem.Where(x => x.TSN == data.TSN && !cisnList.Contains(x.CISN));
            DeleteCheckItemList(delItems);

            // 更新 Template_CheckItem
            UpdateRangeCheckItemList(data);

            await _db.SaveChangesAsync();

            // 建立 Template_CheckItem
            data.SetCheckItemModifiableList(true);
            AddRangeCheckItem(data);
        }
        #endregion

        #region 更新填報項目
        public async Task UpdateReportItemListAsync(IUpdateReportItemList data)
        {
            // 資料驗證
            data.SetReportItemModifiableList();
            ReportItemDataAnnotation(data);

            // 刪除 Template_ReportingItem
            var risnList = data.ReportItemList?.Select(x => x.RISN).ToList() ?? new List<string>();
            var delItems = _db.Template_ReportingItem.Where(x => x.TSN == data.TSN && !risnList.Contains(x.RISN));
            DeleteReportItemList(delItems);

            // 更新 Template_ReportingItem
            UpdateRangeReportItemList(data);

            await _db.SaveChangesAsync();

            // 建立 Template_ReportingItem
            data.SetReportItemModifiableList(true);
            AddRangeReportItem(data);
        }
        #endregion

        #region 獲取一機一卡模板
        public async Task<T> GetOneDeviceOneCardAsync<T>(string TSN) where T : class, new()
        {
            var template = await _db.Template_OneDeviceOneCard.FindAsync(TSN)
                ?? throw new MyCusResException("查無資料！");

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

        #region 刪除一機一卡模板
        public async Task DeleteOneDeviceOneCardAsync(Template_OneDeviceOneCard data)
        {
            // 刪除使用該模板之巡檢預設順序
            var delOrder = data.EquipmentInfo
                .SelectMany(x => x.RFID)
                .SelectMany(x => x.InspectionDefaultOrder);
            // 刪除RFID相關巡檢路線
            await _eMgmtService.DeleteRFIDInspectionOrderAsync(delOrder);

            // 刪除模板
            _db.Template_OneDeviceOneCard.Remove(data);
        }
        #endregion

        #region 批次刪除增設基本資料欄位
        public void DeleteAddFieldList(IEnumerable<Template_AddField> data)
        {
            if (data?.Any() != true)
                return;

            // 刪除關聯的 Equipment_AddFieldValue
            _eMgmtService.DeleteAddFieldValueList(data.SelectMany(x => x.Equipment_AddFieldValue));

            // 刪除 Template_AddField
            _db.Template_AddField.RemoveRange(data);
        }
        #endregion

        #region 批次刪除保養項目設定
        public void DeleteMaintainItemList(IEnumerable<Template_MaintainItemSetting> data)
        {
            if (data?.Any() != true) 
                return;

            // 刪除 1.關聯的 Equipment_MaintainItemValue 及 2.相關待派工的定期保養單
            _eMgmtService.DeleteMaintainItemValueList(data.SelectMany(x => x.Equipment_MaintainItemValue));

            // 移除已派工的定期保養單關聯
            data.SelectMany(x => x.Equipment_MaintenanceForm)
                .Where(x => !IsEnumEqualToStr(x.Status, MaintenanceFormStatus.ToDo))
                .ToList()
                .ForEach(x =>
                {
                    x.MISSN = null;
                    _maintainService.UpdateMaintainForm(x);
                });

            // 刪除 Template_MaintainItemSetting
            _db.Template_MaintainItemSetting.RemoveRange(data);
        }
        #endregion

        #region 批次刪除檢查項目
        public void DeleteCheckItemList(IEnumerable<Template_CheckItem> data)
        {
            if (data?.Any() != true) 
                return;

            // 刪除 Template_CheckItem
            _db.Template_CheckItem.RemoveRange(data);
        }
        #endregion

        #region 批次刪除填報項目
        public void DeleteReportItemList(IEnumerable<Template_ReportingItem> data)
        {
            if (data?.Any() != true) 
                return;

            // 刪除 Template_ReportingItem
            _db.Template_ReportingItem.RemoveRange(data);
        }
        #endregion

        //-----資料驗證
        #region Template_OneDeviceOneCard資料驗證
        private void DeviceCardDataAnnotation(ICreateDeviceCard data, string tsn = null)
        {
            // 不可重複：模板名稱
            var templates = string.IsNullOrEmpty(tsn) ? 
                _db.Template_OneDeviceOneCard : 
                _db.Template_OneDeviceOneCard.Where(x => x.TSN != tsn);

            if (templates.Select(x => x.SampleName).ToList().Contains(data.SampleName))
                throw new MyCusResException("模板名稱已被使用！");
        }
        #endregion

        #region AddField資料驗證
        private void AddFieldDataAnnotation(IAddFieldModifiableList data/*, bool compareOrigin = false*/)
        {
            // 驗證新增設基本欄位(data包含既有的欄位)
            ValidateList(data.AFNameList, "增設基本欄位", 100);
            // 與既有欄位進行比對
            //if (compareOrigin && await _db.Template_AddField.Where(x => x.TSN == data.TSN).AnyAsync(x => data.AFNameList.Contains(x.FieldName)))
            //    throw new MyCusResException("增設基本欄位不可重複！");
        }
        #endregion

        #region MaintainItem資料驗證
        private void MaintainItemDataAnnotation(IMaintainItemModifiableList data)
        {
            // 驗證新增設基本欄位(data包含既有的欄位)
            ValidateList(data.MINameList, "保養項目", 100);
            // 與既有欄位進行比對
            //if (await _db.Template_MaintainItemSetting.Where(x => x.TSN == data.TSN).AnyAsync(x => data.MINameList.Contains(x.MaintainName)))
            //    throw new MyCusResException("保養項目名稱不可重複！");
        }
        #endregion

        #region CheckItem資料驗證
        private void CheckItemDataAnnotation(ICreateCheckItemList data)
        {
            // 當檢查項目不為空，則檢查頻率為必填
            if (data.Frequency == null && data.CINameList?.Any() == true)
                throw new MyCusResException("請填寫檢查頻率！");
            // 驗證新增設基本欄位(data包含既有的欄位)
            ValidateList(data.CINameList, "檢查項目", 100);
            // 與既有欄位進行比對
            //if (await _db.Template_CheckItem.Where(x => x.TSN == data.TSN).AnyAsync(x => data.CINameList.Contains(x.CheckItemName)))
            //    throw new MyCusResException("檢查項目名稱不可重複！");
        }
        #endregion

        #region ReportItem資料驗證
        private void ReportItemDataAnnotation(ICreateReportItemList data)
        {
            // 當填報項目不為空，則檢查頻率為必填
            if (data.Frequency == null && data.RIList?.Any() == true)
                throw new MyCusResException("請填寫檢查頻率！");
            ValidateList(data.RIList?.Select(x => x.RIName), "填報項目", 100);
            // 與既有欄位進行比對
            //var riNameList = data.RIList.Select(r => r.RIName).ToList();
            //if (await _db.Template_ReportingItem
            //    .AnyAsync(x => x.TSN == data.TSN && riNameList.Contains(x.ReportingItemName)))
            //    throw new MyCusResException("檢查項目名稱不可重複！");
        }
        #endregion

        //-----其他
        #region 產生一機一卡模板唯一編碼
        /// <summary>
        /// 產生一機一卡模板唯一編碼
        /// </summary>
        /// <returns>唯一編碼</returns>
        /// <remarks>唯一編碼規則為"!{yyMMdd}%{2}"</remarks>
        public async Task<string> GenerateTsnAsync()
        {
            string format = "!{yyMMdd}%{2}";
            string emptySN = new string('0', 8); // 產生8位數的'0'

            // 前一筆資料
            var latest = await _db.Template_OneDeviceOneCard.OrderByDescending(x => x.TSN).FirstOrDefaultAsync();
            // SN碼
            return latest == null ?
                ComFunc.CreateNextID(format, emptySN) :
                ComFunc.CreateNextID(format, latest.TSN);
        }
        #endregion

        #region 獲取最後一個編碼
        public string GetLatestIdWithSameTsn(string tsn, IQueryable<dynamic> query, string fieldName)
        {
            return query.Where($"TSN == @0", tsn)
                .OrderBy($"{fieldName} descending")  // 動態排序
                .Select($"{fieldName}")  // 動態選擇欄位
                .FirstOrDefault()?.ToString();
        }
        #endregion

        #region 產生唯一編碼
        /// <summary>
        /// 產生 Template_AddField/ Template_MaintainItemSetting/ Template_CheckItem/ Template_ReportingItem 唯一編碼
        /// </summary>
        /// <param name="tsn">TSN碼</param>
        /// <param name="latestId">前一筆資料唯一編碼</param>
        /// <returns>唯一編碼</returns>
        /// <remarks>唯一編碼規則為"${TSN}%{3}"</remarks>
        public string GenerateUniqueIdByTSN(string tsn, string latestId)
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
        private void AddRangeAddField(ICreateAddFieldList data)
        {
            if (data.AFNameList == null || !data.AFNameList.Any())
                return;

            // Fetch最後一個Id
            string latestId = GetLatestIdWithSameTsn(data.TSN, _db.Template_AddField, "AFSN");

            _db.Template_AddField.AddRange(Helper.AddOrUpdateList(
                data.AFNameList,
                data.TSN,
                latestId,
                (esn, lastId) => GenerateUniqueIdByTSN(data.TSN, lastId),
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
        private void AddRangeMaintainItem(ICreateMaintainItemList data, out List<Template_MaintainItemSetting> list)
        {
            if (data.MINameList == null || !data.MINameList.Any())
            {
                list = null;
                return;
            }

            // Fetch最後一個Id
            string latestId = GetLatestIdWithSameTsn(data.TSN, _db.Template_MaintainItemSetting, "MISSN");

            list = Helper.AddOrUpdateList(
                data.MINameList,
                data.TSN,
                latestId,
                (esn, lastId) => GenerateUniqueIdByTSN(data.TSN, lastId),
                (x, e, lastId, newId) => new Template_MaintainItemSetting
                {
                    MISSN = newId,  // 使用新生成的Id
                    TSN = e,
                    MaintainName = x
                }
            ).ToList();

            _db.Template_MaintainItemSetting.AddRange(list);
        }
        #endregion

        #region 批次新增 Template_CheckItem 
        /// <summary>
        /// 批次新增設備一機一卡檢查項目
        /// </summary>
        /// <param name="data">包含一機一卡檢查項目名稱列表及一機一卡模板編碼(TSN)</param>
        /// <returns>無回傳</returns>
        private void AddRangeCheckItem(ICreateCheckItemList data)
        {
            if (data.CINameList == null || !data.CINameList.Any())
                return;

            // 只調用一次 GetLatestIdWithSameTsnAsync
            string latestId = GetLatestIdWithSameTsn(data.TSN, _db.Template_CheckItem, "CISN");

            _db.Template_CheckItem.AddRange(Helper.AddOrUpdateList(
                data.CINameList,
                data.TSN,
                latestId,
                (esn, lastId) => GenerateUniqueIdByTSN(data.TSN, lastId),
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
        private void AddRangeReportItem(ICreateReportItemList data)
        {
            if (data.RIList == null || !data.RIList.Any())
                return;

            // Fetch最後一個Id
            string latestId = GetLatestIdWithSameTsn(data.TSN, _db.Template_ReportingItem, "RISN");

            _db.Template_ReportingItem.AddRange(Helper.AddOrUpdateList(
                data.RIList,
                data.TSN,
                latestId,
                (esn, lastId) => GenerateUniqueIdByTSN(data.TSN, lastId),
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

        #region 批次更新 Template_AddField
        /// <summary>
        /// 批次更新設備一機一卡檢查項目
        /// </summary>
        /// <param name="data">包含一機一卡檢查項目名稱列表及一機一卡模板編碼(TSN)</param>
        /// <returns>無回傳</returns>
        private void UpdateRangeAddField(IUpdateAddFieldList data)
        {
            var targetList = data?.AddItemList?.Where(x => !string.IsNullOrEmpty(x.AFSN))
                ?? new List<AddFieldDetailModel>();

            foreach (var item in targetList)
            {
                var field = _db.Template_AddField.Find(item.AFSN) ?? throw new MyCusResException("AFSN不存在！");
                field.FieldName = item.Value;
                _db.Template_AddField.AddOrUpdate(field);
            }
        }
        #endregion

        #region 批次更新 Template_MaintainItemSetting
        /// <summary>
        /// 批次更新設備一機一卡保養項目設定
        /// </summary>
        /// <param name="data">包含一機一卡保養項目名稱列表及一機一卡模板編碼(TSN)</param>
        /// <returns>無回傳</returns>
        private void UpdateRangeMaintainItemList(in IUpdateMaintainItemList data, bool ignoreEmpty = true)
        {
            var targetList = data.MaintainItemList ?? new List<MaintainItemDetailModel>();
            if (ignoreEmpty)
                targetList = targetList.Where(x => !string.IsNullOrEmpty(x.MISSN)).ToList();

            foreach (var item in targetList)
            {
                var field = _db.Template_MaintainItemSetting.Find(item.MISSN) ?? throw new MyCusResException("MISSN不存在！");
                field.MaintainName = item.Value;
                _db.Template_MaintainItemSetting.AddOrUpdate(field);
            }
        }
        #endregion

        #region 批次更新 Template_CheckItem
        /// <summary>
        /// 批次更新設備一機一卡基本資料欄位
        /// </summary>
        /// <param name="data">包含一機一卡基本資料欄位名稱列表及一機一卡模板編碼(TSN)</param>
        /// <returns>無回傳</returns>
        private void UpdateRangeCheckItemList(in IUpdateCheckItemList data, bool ignoreEmpty = true)
        {
            var targetList = data.CheckItemList ?? new List<CheckItemDetailModel>();
            if (ignoreEmpty)
                targetList = targetList.Where(x => !string.IsNullOrEmpty(x.CISN)).ToList();

            foreach (var item in targetList)
            {
                var field = _db.Template_CheckItem.Find(item.CISN) ?? throw new MyCusResException("CISN不存在！");
                field.CheckItemName = item.Value;
                _db.Template_CheckItem.AddOrUpdate(field);
            }
        }
        #endregion

        #region 批次更新 Template_ReportingItem
        /// <summary>
        /// 批次更新設備一機一卡基本資料欄位
        /// </summary>
        /// <param name="data">包含一機一卡基本資料欄位名稱列表及一機一卡模板編碼(TSN)</param>
        /// <returns>無回傳</returns>
        private void UpdateRangeReportItemList(in IUpdateReportItemList data, bool ignoreEmpty = true)
        {
            var targetList = data.ReportItemList ?? new List<ReportItemDetailModel>();
            if (ignoreEmpty)
                targetList = targetList.Where(x => !string.IsNullOrEmpty(x.RISN)).ToList();

            foreach (var item in targetList)
            {
                var field = _db.Template_ReportingItem.Find(item.RISN) ?? throw new MyCusResException("RISN不存在！");
                field.ReportingItemName = item.Value;
                _db.Template_ReportingItem.AddOrUpdate(field);
            }
        }
        #endregion

        #region Helper
        private void ValidateList(IEnumerable<string> list, string fieldName, uint listMaxLength)
        {
            if (list != null && list.Any())
            {
                // List長度限制
                if (list.Count() > listMaxLength)
                    throw new MyCusResException($"{fieldName}不可超過{listMaxLength}項！");
                // 不可重複：名稱
                if (list.Count() != list.Distinct().Count())
                    throw new MyCusResException($"{fieldName}名稱不可重複！");
            }
        }
        #endregion
    }
}