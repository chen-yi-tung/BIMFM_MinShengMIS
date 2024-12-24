using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
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
        public async Task<string> CreateOneDeviceOneCardAsync(ICreateDeviceCard data)
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
            if (!await _db.Template_OneDeviceOneCard.AnyAsync(x => x.TSN == data.TSN))
                throw new MyCusResException("模板不存在!");

            // 資料驗證
            DeviceCardDataAnnotation(data, data.TSN);

            // 更新 Template_OneDeviceOneCard
            Template_OneDeviceOneCard update = (data as DeviceCardEditViewModel)
                .ToDto<DeviceCardEditViewModel, Template_OneDeviceOneCard>();

            _db.Template_OneDeviceOneCard.AddOrUpdate(update);
        }
        #endregion

        #region 更新增設基本資料欄位
        public async Task UpdateAddFieldListAsync(IUpdateAddFieldList data)
        {
            // 資料驗證
            AddFieldDataAnnotation(new AddFieldModifiableListInstance(data as DeviceCardEditViewModel));

            // 刪除 Template_AddField
            var afsnList = data.AddItemList?.Select(x => x.AFSN).ToList() ?? new List<string>();
            DeleteAddFieldList delTarget = new DeleteAddFieldList
            {
                AFSN = _db.Template_AddField
                    .Where(x => x.TSN == data.TSN && !afsnList.Contains(x.AFSN))
                    .Select(x => x.AFSN)
                    .ToList()
            };
            DeleteAddFieldList(delTarget);

            // 更新 Template_AddField
            UpdateRangeAddField(data);

            await _db.SaveChangesAsync();

            // 建立 Template_AddField
            AddRangeAddField(new AddFieldModifiableListInstance(data as DeviceCardEditViewModel, true));
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
            MaintainItemDataAnnotation(new MaintainItemModifiableListInstance(data as DeviceCardEditViewModel, noEquipmentUsed:true, equipmentUsed: true));

            #region MaintainItemDetailModel 既有保養項目/未有設備使用之保養項目
            // 刪除 Template_MaintainItemSetting
            var missnList = data.MaintainItemList?.Select(x => x.MISSN).ToList() ?? new List<string>();
            DeleteMaintainItemList(new DeleteMaintainItemList
            {
                MISSN = _db.Template_MaintainItemSetting
                    .Where(x => x.TSN == data.TSN && !missnList.Contains(x.MISSN))
                    .Select(x => x.MISSN)
                    .ToList()
            });

            // 更新 Template_MaintainItemSetting
            UpdateRangeMaintainItemList(data);

            await _db.SaveChangesAsync();

            // 建立 Template_MaintainItemSetting
            AddRangeMaintainItem(new MaintainItemModifiableListInstance(data as DeviceCardEditViewModel, onlyEmptyMISSN: true, noEquipmentUsed: true), out _);
            #endregion

            #region AddEquipmentUsedMaintainItem 新增之保養項目及其於各設備值
            // 具新增之保養項目
            if (data.AddMaintainItemList?.Any() == true)
            {
                // 建立 Template_MaintainItemSetting
                AddRangeMaintainItem(new MaintainItemModifiableListInstance(data as DeviceCardEditViewModel, equipmentUsed: true), out var newItems);

                await _db.SaveChangesAsync();

                // 建立 Equipment_MaintainItemValue
                if (newItems != null && newItems.Any())
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
                        await _eMgmtService.CreateEquipmentMaintainItemsValue(value);
                }
            }
            #endregion

        }
        #endregion

        #region 更新檢查項目
        public async Task UpdateCheckItemListAsync(IUpdateCheckItemList data)
        {
            // 資料驗證
            CheckItemDataAnnotation(new CheckItemModifiableListInstance(data as DeviceCardEditViewModel));

            // 刪除 Template_CheckItem
            var cisnList = data.CheckItemList?.Select(x => x.CISN).ToList() ?? new List<string>();
            DeleteCheckItemList delTarget = new DeleteCheckItemList
            {
                CISN = _db.Template_CheckItem
                    .Where(x => x.TSN == data.TSN && !cisnList.Contains(x.CISN))
                    .Select(x => x.CISN)
                    .ToList()
            };
            DeleteCheckItemList(delTarget);

            // 更新 Template_CheckItem
            UpdateRangeCheckItemList(data);

            await _db.SaveChangesAsync();

            // 建立 Template_CheckItem
            AddRangeCheckItem(new CheckItemModifiableListInstance(data as DeviceCardEditViewModel, true));
        }
        #endregion

        #region 更新填報項目
        public async Task UpdateReportItemListAsync(IUpdateReportItemList data)
        {
            // 資料驗證
            ReportItemDataAnnotation(new ReportItemModifiableListInstance(data as DeviceCardEditViewModel));

            // 刪除 Template_ReportingItem
            var risnList = data.ReportItemList?.Select(x => x.RISN).ToList() ?? new List<string>();
            DeleteReportItemList delTarget = new DeleteReportItemList
            {
                RISN = _db.Template_ReportingItem
                    .Where(x => x.TSN == data.TSN && !risnList.Contains(x.RISN))
                    .Select(x => x.RISN)
                    .ToList()
            };
            DeleteReportItemList(delTarget);

            // 更新 Template_ReportingItem
            UpdateRangeReportItemList(data);

            await _db.SaveChangesAsync();

            // 建立 Template_ReportingItem
            AddRangeReportItem(new ReportItemModifiableListInstance(data as DeviceCardEditViewModel, true));
        }
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
        private void DeviceCardDataAnnotation(ICreateDeviceCard data, string tsn = null)
        {
            // 不可重複：模板名稱
            var templates = string.IsNullOrEmpty(tsn) ? 
                _db.Template_OneDeviceOneCard : 
                _db.Template_OneDeviceOneCard.Where(x => x.TSN != tsn);

            if (templates.Select(x => x.SampleName).ToList().Contains(data.SampleName))
                throw new MyCusResException("模板名稱已被使用!");
        }
        #endregion

        #region AddField資料驗證
        private void AddFieldDataAnnotation(IAddFieldModifiableList data/*, bool compareOrigin = false*/)
        {
            // 驗證新增設基本欄位(data包含既有的欄位)
            ValidateList(data.AFNameList, "增設基本欄位", 100);
            // 與既有欄位進行比對
            //if (compareOrigin && await _db.Template_AddField.Where(x => x.TSN == data.TSN).AnyAsync(x => data.AFNameList.Contains(x.FieldName)))
            //    throw new MyCusResException("增設基本欄位不可重複!");
        }
        #endregion

        #region MaintainItem資料驗證
        private void MaintainItemDataAnnotation(IMaintainItemModifiableList data)
        {
            // 驗證新增設基本欄位(data包含既有的欄位)
            ValidateList(data.MINameList, "保養項目", 100);
            // 與既有欄位進行比對
            //if (await _db.Template_MaintainItemSetting.Where(x => x.TSN == data.TSN).AnyAsync(x => data.MINameList.Contains(x.MaintainName)))
            //    throw new MyCusResException("保養項目名稱不可重複!");
        }
        #endregion

        #region CheckItem資料驗證
        private void CheckItemDataAnnotation(ICreateCheckItemList data)
        {
            // 當檢查項目不為空，則檢查頻率為必填
            if (data.Frequency == null && data.CINameList?.Any() == true)
                throw new MyCusResException("請填寫檢查頻率!");
            // 驗證新增設基本欄位(data包含既有的欄位)
            ValidateList(data.CINameList, "檢查項目", 100);
            // 與既有欄位進行比對
            //if (await _db.Template_CheckItem.Where(x => x.TSN == data.TSN).AnyAsync(x => data.CINameList.Contains(x.CheckItemName)))
            //    throw new MyCusResException("檢查項目名稱不可重複!");
        }
        #endregion

        #region ReportItem資料驗證
        private void ReportItemDataAnnotation(ICreateReportItemList data)
        {
            // 當填報項目不為空，則檢查頻率為必填
            if (data.Frequency == null && data.RIList?.Any() == true)
                throw new MyCusResException("請填寫檢查頻率!");
            ValidateList(data.RIList?.Select(x => x.RIName), "填報項目", 100);
            // 與既有欄位進行比對
            //var riNameList = data.RIList.Select(r => r.RIName).ToList();
            //if (await _db.Template_ReportingItem
            //    .AnyAsync(x => x.TSN == data.TSN && riNameList.Contains(x.ReportingItemName)))
            //    throw new MyCusResException("檢查項目名稱不可重複!");
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
        /// <param name="fieldName">唯一編碼欄位名稱</param>
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
        private void UpdateRangeAddField(in IUpdateAddFieldList data, bool ignoreEmpty = true)
        {
            var targetList = data.AddItemList ?? new List<AddFieldDetailModel>();
            if (ignoreEmpty)
                targetList = targetList.Where(x => !string.IsNullOrEmpty(x.AFSN)).ToList();

            foreach (var item in targetList)
            {
                var field = _db.Template_AddField.Find(item.AFSN) ?? throw new MyCusResException("AFSN不存在!");
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
                var field = _db.Template_MaintainItemSetting.Find(item.MISSN) ?? throw new MyCusResException("MISSN不存在!");
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
                var field = _db.Template_CheckItem.Find(item.CISN) ?? throw new MyCusResException("CISN不存在!");
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
                var field = _db.Template_ReportingItem.Find(item.RISN) ?? throw new MyCusResException("RISN不存在!");
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
                    throw new MyCusResException($"{fieldName}不可超過{listMaxLength}項!");
                // 不可重複：名稱
                if (list.Count() != list.Distinct().Count())
                    throw new MyCusResException($"{fieldName}名稱不可重複!");
            }
        }
        #endregion
    }
}