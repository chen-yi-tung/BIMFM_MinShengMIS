using MinSheng_MIS.Models;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using System.Linq;
using MinSheng_MIS.Models.ViewModels;
using System.Threading.Tasks;
using System.Data.Entity;
using Microsoft.Ajax.Utilities;
using System.Data.Entity.Migrations;

namespace MinSheng_MIS.Services
{
    public class SamplePath_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly RFIDService _rfidService;
        private readonly SampleSchedule_ManagementService _sampleScheduleService;
        private readonly PlanManagementService _planService;

        public SamplePath_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _rfidService = new RFIDService(_db);
            _sampleScheduleService = new SampleSchedule_ManagementService(_db);
            _planService = new PlanManagementService(_db);
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
                    RFIDViewName = x.Floor_Info.ViewName,
                    Location_X = x.Location_X,
                    Location_Y = x.Location_Y,
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

        #region 新增巡檢路線模板 InspectionPathSample
        public async Task<string> CreateSamplePathAsync(ICreateSamplePathInfo data)
        {
            // 資料驗證
            PathSampleDataAnnotation((IEditSamplePathInfo)data);

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

        #region 建立巡檢設備順序 InspectionDefaultOrder
        public void CreateInspectionDefaultOrders(IDefaultOrderModifiableList data)
        {
            // 資料驗證
            DefaultOrderDataAnnotation(data);

            // 批次建立 InspectionDefaultOrder
            AddRangeDefaultOrder(data);
        }
        #endregion

        #region 獲取巡檢路線模板 InspectionPathSample
        public T GetSamplePath<T>(string planPathSN)
            where T : class, new()
        {
            return GetSamplePath<T>(_db, planPathSN);
        }

        public static T GetSamplePath<T>(Bimfm_MinSheng_MISEntities db, string planPathSN)
            where T : class, new()
        {
            var sample = db.InspectionPathSample.Find(planPathSN)
                ?? throw new MyCusResException("查無資料！");

            return sample.ToDto<InspectionPathSample, T>();
        }
        #endregion

        #region 獲取巡檢設備順序 InspectionDefaultOrder
        public List<IInspectionRFIDs> GetDefaultOrderRFIDInfoList(string planPathSN)
        {
            var rfids = _db.InspectionDefaultOrder.Where(x => x.PlanPathSN == planPathSN)
                .Select(x => x.RFID)
                .AsEnumerable();

            var result = rfids
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
                    Frequency = $"每{x.EquipmentInfo.Template_OneDeviceOneCard.Frequency}小時",
                    RFIDViewName = x.Floor_Info.ViewName.Trim(),
                    Location_X = x.Location_X,
                    Location_Y = x.Location_Y
                });

            return result.Cast<IInspectionRFIDs>().ToList();
        }
        #endregion

        #region 編輯巡檢路線模板 InspectionPathSample
        public async Task EditSamplePathAsync(IEditSamplePathInfo data)
        {
            if (!await _db.InspectionPathSample.AnyAsync(x => x.PlanPathSN == data.PlanPathSN))
                throw new MyCusResException("模板不存在！");

            // 資料驗證
            PathSampleDataAnnotation(data);

            // 更新 InspectionPathSample
            var path = data.ToDto<IEditSamplePathInfo, InspectionPathSample>();

            _db.InspectionPathSample.AddOrUpdate(path);
        }
        #endregion

        #region 編輯巡檢設備順序 InspectionDefaultOrder
        public async Task EditInspectionDefaultOrdersAsync(IDefaultOrderModifiableList data)
        {
            var sample = await _db.InspectionPathSample
                .SingleOrDefaultAsync(x => x.PlanPathSN == data.PlanPathSN)
                ?? throw new MyCusResException("模板不存在！");

            // 資料驗證
            DefaultOrderDataAnnotation(data);

            // 清除 InspectionDefaultOrder
            DeleteInspectionDefaultOrder(sample.InspectionDefaultOrder);

            await _db.SaveChangesAsync();

            // 批次建立 InspectionDefaultOrder
            AddRangeDefaultOrder(data);
        }
        #endregion

        #region 刪除巡檢路線模板 InspectionPathSample
        public async Task DeleteInspectionPathSampleAsync(InspectionPathSample data)
        {
            if (data == null)
                return;

            // 刪除巡檢預設順序
            DeleteInspectionDefaultOrder(data.InspectionDefaultOrder);

            var DailyInspection = data.DailyInspectionSampleContent
                ?.Select(x => x.DailyInspectionSample)
                ?.Distinct()
                .ToList();
            if (DailyInspection != null)
            {
                // 刪除使用路線的巡檢時程安排模板內容
                _sampleScheduleService.DeleteSampleScheduleContents
                    (data.DailyInspectionSampleContent);
                await _db.SaveChangesAsync();
                // 刪除巡檢時程安排模板
                DailyInspection.Where(x => x.DailyInspectionSampleContent?.Any() != true)
                    .ForEach(x => _sampleScheduleService.DeleteInspectionSample(x));
            }

            var plan = data.InspectionPlan_Time
                ?.Select(x => x.InspectionPlan)
                ?.Distinct()
                .ToList();
            if (plan != null)
            {
                // 刪除使用路線的工單巡檢時段及執行人員
                _planService.DeleteInspectionPlanContent(data.InspectionPlan_Time);
                await _db.SaveChangesAsync();
                // 刪除工單
                plan.Where(x => x.InspectionPlan_Time?.Any() != true)
                    .ForEach(x =>
                    {
                        try { _planService.DeleteInspectionPlan(x);}
                        catch { }
                    });
            }

            // 刪除模板
            _db.InspectionPathSample.Remove(data);
        }
        #endregion

        #region 刪除巡檢預設順序 InspectionDefaultOrder
        /// <summary>
        /// 刪除巡檢預設順序
        /// </summary>
        /// <param name="data">IEnumerable of <see cref="InspectionDefaultOrder"/></param>
        public void DeleteInspectionDefaultOrder(IEnumerable<InspectionDefaultOrder> data)
        {
            if (data?.Any() != true)
                return;

            _db.InspectionDefaultOrder.RemoveRange(data);
        }
        #endregion

        //-----資料驗證

        #region InspectionPathSample 資料驗證
        private void PathSampleDataAnnotation(IEditSamplePathInfo data)
        {
            // 不可重複：巡檢路線名稱
            var sample = string.IsNullOrEmpty(data.PlanPathSN) ?
                _db.InspectionPathSample :
                _db.InspectionPathSample.Where(x => x.PlanPathSN != data.PlanPathSN);

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
            if (data.RFIDInternalCodes.Count() >= 100000)
                throw new MyCusResException($"巡檢設備不可超過100000項！"); 
            // 不可重複：RFID內碼
            if (data.RFIDInternalCodes.Count() != data.RFIDInternalCodes.Distinct().Count())
                throw new MyCusResException($"設備RFID不可重複！");
            var equipmentRFIDs = _rfidService
                .GetRFIDQueryByDto<RFID>(x => string.IsNullOrEmpty(x.SARSN))
                .AsEnumerable();
            // 關聯性PK是否存在：RFID內碼
            var InternalCodes = equipmentRFIDs.Select(x => x.RFIDInternalCode);
            if (!data.RFIDInternalCodes.All(x => InternalCodes.Contains(x)))
                throw new MyCusResException("設備RFID不存在！");
            // RFID的設備巡檢頻率是否與模板相同
            if (equipmentRFIDs
                .Where(x => data.RFIDInternalCodes.Contains(x.RFIDInternalCode))
                .Any(x =>
                    x.EquipmentInfo?.Template_OneDeviceOneCard?.Frequency.HasValue != true ||
                    x.EquipmentInfo.Template_OneDeviceOneCard.Frequency != data.Frequency
                ))
                throw new MyCusResException("設備的巡檢頻率與模板不同，請確認後重新送出！");
        }
        #endregion

        //-----其他
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
                new string[] { data.PlanPathSN }, // param
                (format, Len, lastId, sn) => ComFunc.GenerateUniqueSn(format, Len, lastId, sn),
                (item, param, newId) => new InspectionDefaultOrder
                {
                    DefaultOrder = newId,
                    PlanPathSN = param[0],
                    RFIDInternalCode = item,
                }
            ));
        }
        #endregion
    }
}