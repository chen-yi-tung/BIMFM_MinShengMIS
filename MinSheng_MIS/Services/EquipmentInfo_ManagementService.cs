using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Surfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace MinSheng_MIS.Services
{
    public class EquipmentInfo_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly ComFunc _cFunc;

        public EquipmentInfo_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _cFunc = new ComFunc();
        }

        #region 新增設備
        public async Task<string> CreateEquipmentInfoAsync(IUpdateEquipmentInfo data)
        {
            // 資料驗證
            await EquipmentInfoDataAnnotationAsync(data);

            // 建立 EquipmentInfo
            var equipmentInfo = new EquipmentInfo
            {
                ESN = await GenerateEquipmentInfoSNAsync(),
                EName = data.EName,
                NO = data.No,
                FSN = data.FSN,
                Brand = data.Brand,
                Vendor = data.Vendor,
                Model = data.Model,
                ContactPhone = data.ContactPhone,
                OperatingVoltage = data.OperatingVoltage,
                OtherInfo = data.OtherInfo,
                InstallDate = data.InstallDate,
                Memo = data.Memo,
                EState = ((int)UniParams.EState.Normal).ToString(),
                TSN = data.TSN,
                IsDelete = false,
                EPhoto = null // TODO
            };
            _db.EquipmentInfo.Add(equipmentInfo);

            return equipmentInfo.ESN;
        }
        #endregion

        #region 建立設備所有增設欄位值
        public async Task CreateEquipmentAdditionalFieldsValue(IUpdateAddFieldValue data)
        {
            // 資料驗證
            await AddFieldValueDataAnnotationAsync(data);

            // 批次建立 Equipment_AddFieldValue
            AddRangeAddFieldValue(data);
        }
        #endregion

        #region 建立設備所有增設欄位值
        public async Task CreateEquipmentMaintainItemsValue(IUpdateMaintainItemValue data)
        {
            // 資料驗證
            await MaintainItemValueDataAnnotationAsync(data);

            // 批次建立 Equipment_MaintainItemValue
            AddRangeMaintainItemValue(data);
        }
        #endregion

        //-----資料驗證
        #region EquipmentInfo資料驗證
        private async Task EquipmentInfoDataAnnotationAsync(IUpdateEquipmentInfo data)
        {
            var floorSNList = await _db.Floor_Info.Select(x => x.FSN).ToListAsync(); // 取得所有樓層SN列表

            // 不可重複: 設備編號
            if (await _db.EquipmentInfo.Where(x => x.NO == data.No).AnyAsync())
                throw new MyCusResException("設備編號已被使用!");
            // 關聯性PK是否存在: 樓層
            if (!floorSNList.Contains(data.FSN))
                throw new MyCusResException("樓層不存在!");
            // 照片驗證
            if (data.EPhoto != null && data.EPhoto.ContentLength > 0)
            {
                string extension = Path.GetExtension(data.EPhoto.FileName); // 檔案副檔名
                if (!ComFunc.IsConformedForImage(data.EPhoto.ContentType, extension)
                    || ComFunc.IsConformedForPdf(data.EPhoto.ContentType, extension)) // 檔案白名單檢查
                    throw new MyCusResException("非系統可接受的檔案格式!<br>僅支援上傳圖片或PDF!");
            }
        }
        #endregion

        #region AddFieldValue資料驗證
        private async Task AddFieldValueDataAnnotationAsync(IUpdateAddFieldValue data)
        {
            // 關聯性PK是否存在:一機一卡編號
            var template = await _db.Template_OneDeviceOneCard.SingleOrDefaultAsync(x => x.TSN == data.TSN) 
                ?? throw new MyCusResException("模板不存在!");

            // 確認是否符合一機一卡模板當前欄位
            if (!AreListsEqualIgnoreOrder(template.Template_AddField.Select(x => x.AFSN).ToList(), data.AddFieldList.Select(x => x.AFSN).ToList()))
                throw new MyCusResException("模板已更新，請重新填寫!");
        }
        #endregion

        #region MaintainItemValue資料驗證
        private async Task MaintainItemValueDataAnnotationAsync(IUpdateMaintainItemValue data)
        {
            // 關聯性PK是否存在:一機一卡編號
            var template = await _db.Template_OneDeviceOneCard.SingleOrDefaultAsync(x => x.TSN == data.TSN)
                ?? throw new MyCusResException("模板不存在!");

            // 確認是否符合一機一卡模板當前保養項目
            if (!AreListsEqualIgnoreOrder(template.Template_MaintainItemSetting.Select(x => x.MISSN).ToList(), data.MaintainItemList.Select(x => x.MISSN).ToList()))
                throw new MyCusResException("模板已更新，請重新填寫!");

            // 保養週期代碼是否於系統可辨識範圍內
            if (!data.MaintainItemList.Select(x => x.Period).AsEnumerable().All(p => Surface.MaintainPeriod().ContainsKey(p)))
                throw new MyCusResException("保養週期不存在!");
        }
        #endregion

        //-----其他
        #region 產生設備資料唯一編碼
        /// <summary>
        /// 產生設備資料唯一編碼
        /// </summary>
        /// <returns>唯一編碼</returns>
        public async Task<string> GenerateEquipmentInfoSNAsync()
        {
            // 前一筆資料
            var latestInfo = await _db.EquipmentInfo.OrderByDescending(x => x.ESN).FirstOrDefaultAsync();
            // SN碼
            return latestInfo == null ?
                ComFunc.CreateNextID("E!{yyMMdd}%{5}", "000000000000") :
                ComFunc.CreateNextID("E!{yyMMdd}%{5}", latestInfo.ESN);
        }
        #endregion

        #region 批次新增 Equipment_AddFieldValue
        /// <summary>
        /// 批次新增設備一機一卡基本資料欄位填值資料
        /// </summary>
        /// <param name="data">包含一機一卡基本資料欄位填值資料列表(AddFieldList)及設備資料編碼(ESN)</param>
        /// <returns>無回傳</returns>
        private void AddRangeAddFieldValue(IUpdateAddFieldValue data)
        {
            _db.Equipment_AddFieldValue.AddRange(Helper.AddOrUpdateList(data.AddFieldList, data.ESN, (x, e) => new Equipment_AddFieldValue
            {
                EAFVSN = $"{e}{x.AFSN}",
                ESN = e,
                AFSN = x.AFSN,
                Value = x.Value
            }));
        }
        #endregion

        #region 批次新增 Equipment_MaintainItemValue
        /// <summary>
        /// 批次新增設備一機一卡保養項目填值資料
        /// </summary>
        /// <param name="data">包含一機一卡保養項目填值資料列表(MaintainItemList)及設備資料編碼(ESN)</param>
        /// <returns>無回傳</returns>
        private void AddRangeMaintainItemValue(IUpdateMaintainItemValue data)
        {
            _db.Equipment_MaintainItemValue.AddRange(Helper.AddOrUpdateList(data.MaintainItemList, data.ESN, (x, e) => new Equipment_MaintainItemValue
            {
                EMIVSN = $"{e}{x.MISSN}",
                ESN = e,
                MISSN = x.MISSN,
                Period = x.Period,
                lastMaintainDate = null,
                NextMaintainDate = x.NextMaintainDate,
                IsCreateForm = false
            }));
        }
        #endregion

        #region Helper
        /// <summary>
        /// 比較2個List忽略排序後的內容是否完全相等
        /// </summary>
        /// <typeparam name="T">List類型</typeparam>
        /// <param name="list1">List1</param>
        /// <param name="list2">List2</param>
        /// <returns></returns>
        private bool AreListsEqualIgnoreOrder<T>(List<T> list1, List<T> list2)
        {
            if (list1 == null && list2 == null)
                return true;

            if (list1 == null || list2 == null || list1.Count != list2.Count)
                return false;

            // 排序後比較
            return list1.OrderBy(x => x).SequenceEqual(list2.OrderBy(x => x));
        }
        #endregion
    }
}