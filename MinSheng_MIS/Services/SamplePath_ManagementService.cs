using MinSheng_MIS.Models;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using System.Linq;
using MinSheng_MIS.Models.ViewModels;
using System.Threading.Tasks;
using System.Data.Entity;

namespace MinSheng_MIS.Services
{
    public class SamplePath_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly RFIDService _rfidService;
        private readonly EquipmentInfo_ManagementService _eMgmtService;

        public SamplePath_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _rfidService = new RFIDService(_db);
            _eMgmtService = new EquipmentInfo_ManagementService(_db);
        }

        #region 可新增之巡檢設備RFID
        public GridResult<InspectionRFIDsViewModel> GetJsonForGrid_EquipmentRFIDs(EquipmentRFIDSearchParamViewModel data)
        {
            #region 依據查詢字串檢索資料表
            var query = _rfidService.GetRFIDQueryByDto<RFIDServiceQueryModel>(x => string.IsNullOrEmpty(x.SARSN))
                .Select(x => new RFIDQueryModel
                {
                    InternalCode = x.RFIDInternalCode,
                    ExternalCode = x.RFIDExternalCode,
                    EName = x.EquipmentInfo.EName,
                    NO = x.EquipmentInfo.NO,
                    ASN = x.Floor_Info.ASN.ToString(),
                    RFIDArea = x.Floor_Info.AreaInfo.Area,
                    FSN = x.Floor_Info.FSN,
                    RFIDFloor = x.Floor_Info.FloorName,
                    Brand = x.EquipmentInfo.Brand,
                    Model = x.EquipmentInfo.Model,
                    RFIDMemo = x.Memo,
                    RFIDName = x.Name,
                    Frequency = x.EquipmentInfo.Template_OneDeviceOneCard.Frequency,
                });

            // 篩選已選擇之RFID
            if (data.InternalCodes?.Any() == true)
                query = query.Where(x => !data.InternalCodes.Contains(x.InternalCode));
            // 巡檢頻率
            if (data.Frequency.HasValue)
                query = query.Where(x => x.Frequency == data.Frequency.Value);
            // RFID內碼 (模糊查詢)
            if (!string.IsNullOrEmpty(data.InternalCode))
                query = query.Where(x => x.InternalCode.Contains(data.InternalCode));
            // RFID外碼 (模糊查詢)
            if (!string.IsNullOrEmpty(data.ExternalCode))
                query = query.Where(x => x.ExternalCode.Contains(data.ExternalCode));
            // 設備名稱 (模糊查詢)
            if (!string.IsNullOrEmpty(data.EName))
                query = query.Where(x => x.EName.Contains(data.EName));
            // 設備編號 (模糊查詢)
            if (!string.IsNullOrEmpty(data.NO))
                query = query.Where(x => x.NO.Contains(data.NO));
            // 棟別
            if (!string.IsNullOrEmpty(data.RFIDArea))
                query = query.Where(x => x.ASN == data.RFIDArea);
            // 樓層
            if (!string.IsNullOrEmpty(data.RFIDFloor))
                query = query.Where(x => x.FSN == data.RFIDFloor);
            // 廠牌 (模糊查詢)
            if (!string.IsNullOrEmpty(data.Brand))
                query = query.Where(x => x.Brand.Contains(data.Brand));
            // 型號 (模糊查詢)
            if (!string.IsNullOrEmpty(data.Model))
                query = query.Where(x => x.Model.Contains(data.Model));
            #endregion

            //記住總筆數
            int Total = query.Count();

            #region 排序及切頁
            // 排序
            if (!string.IsNullOrEmpty(data.Sort) && !string.IsNullOrEmpty(data.Order))
                query = query.OrderBy(data.Sort + " " + data.Order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
            else
                query = query.OrderBy(x => x.InternalCode);
            // 切頁
            query = query.Skip((data.Page - 1) * data.Rows).Take(data.Rows);
            #endregion

            var result = new GridResult<InspectionRFIDsViewModel>();
            if (query?.Any() == true)
            {
                result.Rows = query.AsEnumerable()
                    .Select(x =>
                    {
                        var dto = x.ToDto<RFIDQueryModel, InspectionRFIDsViewModel>();
                        dto.Frequency = $"每{x.Frequency}小時";
                        return dto;
                    });
                result.Total = Total;
            }
            return result;
        }
        #endregion

        #region 新增巡檢路線模板
        public async Task<string> CreateSamplePathAsync(ISamplePathModifiableList data)
        {
            // 資料驗證
            PathSampleDataAnnotation(data);

            // 前一筆資料
            var latest = await _db.InspectionPathSample.OrderByDescending(x => x.PlanPathSN).FirstOrDefaultAsync();

            // 建立 InspectionPathSample
            var path = new InspectionPathSample
            {
                //PlanPathSN = await GeneratePlanPathSnAsync(),
                PlanPathSN = ComFunc.GenerateUniqueSn("!{yyMMdd}%{3}", 9, latest.PlanPathSN),
                PathName = data.PathName,
                Frequency = data.Frequency
            };
            _db.InspectionPathSample.Add(path);

            return path.PlanPathSN;
        }
        #endregion

        #region 建立巡檢設備順序
        public void CreateInspectionDefaultOrders(IDefaultOrderModifiableList data)
        {
            // 資料驗證
            DefaultOrderDataAnnotation(data);

            // 批次建立 InspectionDefaultOrder
            AddRangeDefaultOrder(data);
        }
        #endregion

        #region 獲取巡檢路線模板
        public async Task<T> GetSamplePathAsync<T>(string planPathSN) where T : class, new()
        {
            var sample = await _db.InspectionPathSample.FindAsync(planPathSN)
                ?? throw new MyCusResException("查無資料！");

            return sample.ToDto<InspectionPathSample, T>();
        }
        #endregion

        #region 獲取巡檢設備順序
        public List<IInspectionRFIDs> GetDefaultOrderRFIDInfoList(string planPathSN)
        {
            var codes = _db.InspectionDefaultOrder.Where(x => x.PlanPathSN == planPathSN)
                .Select(x => x.RFIDInternalCode)
                .AsEnumerable();

            var result = _rfidService.GetRFIDQueryByDto<RFIDServiceQueryModel>(x => codes.Contains(x.RFIDInternalCode))
                .AsEnumerable()
                .Select(x => new InspectionRFIDsViewModel
                {
                    InternalCode = x.RFIDInternalCode,
                    ExternalCode = x.RFIDExternalCode,
                    RFIDName = x.Name,
                    RFIDArea = x.Floor_Info.AreaInfo.Area,
                    RFIDFloor = x.Floor_Info.FloorName,
                    RFIDMemo = x.Memo,
                    EName = x.EquipmentInfo.EName,
                    NO = x.EquipmentInfo.NO,
                    Brand = x.EquipmentInfo.Brand,
                    Model = x.EquipmentInfo.Model,
                    Frequency = $"每{x.EquipmentInfo.Template_OneDeviceOneCard.Frequency}小時"
                });

            return result.Cast<IInspectionRFIDs>().ToList();
        }
        #endregion

        #region 批次刪除巡檢預設順序 InspectionDefaultOrder
        /// <summary>
        /// 批次刪除巡檢預設順序內的特定設備
        /// </summary>
        /// <param name="esn">特定設備列表</param>
        public void DeleteEquipmentInInspectionPathSample(IEnumerable<string> esnList)
        {
            var inspectItems = esnList
                .SelectMany(e => _rfidService.GetRFIDsByEsn(e))
                .SelectMany(x => x.InspectionDefaultOrder)
                .Distinct();

            _db.InspectionDefaultOrder.RemoveRange(inspectItems);
        }
        #endregion

        //-----資料驗證

        #region InspectionPathSample 資料驗證
        private void PathSampleDataAnnotation(ISamplePathModifiableList data, string planPathSN = null)
        {
            // 不可重複：巡檢路線名稱
            var sample = string.IsNullOrEmpty(planPathSN) ?
                _db.InspectionPathSample :
                _db.InspectionPathSample.Where(x => x.PlanPathSN != planPathSN);

            if (sample.Select(x => x.PathName).AsEnumerable().Contains(data.PathName))
                throw new MyCusResException("巡檢路線名稱已被使用！");
        }
        #endregion

        #region InspectionDefaultOrder 資料驗證
        private void DefaultOrderDataAnnotation(IDefaultOrderModifiableList data)
        {
            if (data.RFIDInternalCodes?.Any() != true)
                throw new MyCusResException($"請新增至少一項巡檢設備！");
            // 長度限制
            if (data.RFIDInternalCodes.Count() > 100000)
                throw new MyCusResException($"巡檢設備不可超過100000項！"); 
            // 不可重複：RFID內碼
            if (data.RFIDInternalCodes.Count() != data.RFIDInternalCodes.Distinct().Count())
                throw new MyCusResException($"RFID內碼不可重複！");
            // 關聯性PK是否存在：RFID內碼
            if (Helper.AreListsEqualIgnoreOrder(
                data.RFIDInternalCodes, 
                _db.RFID.Where(x => string.IsNullOrEmpty(x.SARSN)).Select(x => x.RFIDInternalCode)))
                throw new MyCusResException("設備RFID不存在！");
        }
        #endregion

        //-----其他
        #region 產生巡檢路線模板唯一編碼
        /// <summary>
        /// 產生巡檢路線模板唯一編碼
        /// </summary>
        /// <returns>唯一編碼</returns>
        /// <remarks>唯一編碼規則為"!{yyMMdd}%{3}"</remarks>
        public async Task<string> GeneratePlanPathSnAsync()
        {
            string format = "!{yyMMdd}%{3}";
            string emptySN = new string('0', 9); // 產生9位數的'0'

            // 前一筆資料
            var latest = await _db.InspectionPathSample.OrderByDescending(x => x.PlanPathSN).FirstOrDefaultAsync();
            // SN碼
            return latest == null ?
                ComFunc.CreateNextID(format, emptySN) :
                ComFunc.CreateNextID(format, latest.PlanPathSN);
        }
        #endregion

        #region 產生唯一編碼
        /// <summary>
        /// 產生 InspectionDefaultOrder 唯一編碼
        /// </summary>
        /// <param name="planPathSN">PlanPathSN碼</param>
        /// <param name="latestId">前一筆資料唯一編碼</param>
        /// <returns>唯一編碼</returns>
        /// <remarks>唯一編碼規則為"${PlanPathSN}%{5}"</remarks>
        public string GenerateUniqueIdByPlanPathSn(string planPathSN, string latestId)
        {
            string format = planPathSN + "%{5}";
            string emptySN = new string('0', 14); // 產生14位數的'0'

            // SN碼
            return latestId == null ?
                ComFunc.CreateNextID(format, emptySN) :
                ComFunc.CreateNextID(format, latestId);
        }
        #endregion

        #region 批次新增 InspectionDefaultOrder
        /// <summary>
        /// 批次新增巡檢設備預設順序
        /// </summary>
        /// <param name="data">包含設備RFID內碼列表及巡檢路線編號(PlanPathSN)</param>
        /// <returns>無回傳</returns>
        private void AddRangeDefaultOrder(IDefaultOrderModifiableList data)
        {
            if (data.RFIDInternalCodes?.Any() != true)
                return;

            // 不Fetch最後一個Id，每次變更都全部刪除重新建立
            string latestId = null;

            _db.InspectionDefaultOrder.AddRange(Helper.AddOrUpdateList(
                data.RFIDInternalCodes,
                data.PlanPathSN,
                latestId,
                "${SN}%{5}",
                14,
                (format, Len, lastId, sn) => ComFunc.GenerateUniqueSn(format, Len, lastId, sn),
                (item, sn, newId) => new InspectionDefaultOrder
                {
                    DefaultOrder = newId,
                    PlanPathSN = sn,
                    RFIDInternalCode = item,
                }
            ));
        }
        #endregion
    }
}