using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class OneDeviceOneCard_ManagementController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly OneDeviceOneCard_ManagementService _dCardService;
        private readonly EquipmentInfo_ManagementService _eMgmtService;

        public OneDeviceOneCard_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _dCardService = new OneDeviceOneCard_ManagementService(_db);
            _eMgmtService = new EquipmentInfo_ManagementService(_db);
        }

        #region 一機一卡模板 管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 一機一卡模板 新增
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateDeviceCard(DeviceCardCreateViewModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 建立 Template_OneDeviceOneCard
                string tsn = await _dCardService.CreateOneDeviceOneCardAsync(data);
                if (data.Frequency.HasValue && data.CheckItemList?.Any() != true && data.ReportItemList?.Any() != true)
                    throw new MyCusResException("請至少填入一筆檢查項目或填報項目!");

                // 建立 Template_AddField (非必填)
                if (data.AddItemList?.Any() == true)
                    await _dCardService.CreateAddFieldListAsync(data.ConvertToUpdateAddFieldList(tsn));

                // 建立 Template_MaintainItemSetting (非必填)
                if (data.MaintainItemList?.Any() == true)
                    await _dCardService.CreateMaintainItemListAsync(data.ConvertToUpdateMaintainItemList(tsn));

                // 建立 Template_CheckItem (非必填)
                if (data.CheckItemList?.Any() == true)
                    await _dCardService.CreateCheckItemListAsync(data.ConvertToUpdateCheckItemList(tsn));

                // 建立 Template_ReportingItem (非必填)
                if (data.ReportItemList?.Any() == true)
                    await _dCardService.CreateReportItemListAsync(data.ConvertToUpdateReportItemList(tsn));

                await _db.SaveChangesAsync();

                return Content("Succeed");
            }
            catch (MyCusResException ex)
            {
                return Content($"</br>{ex.Message}", "application/json; charset=utf-8");
            }
            catch (Exception)
            {
                return Content("</br>系統異常!", "application/json; charset=utf-8");
            }
        }
        #endregion

        #region 一機一卡模板 編輯 TODO
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public async Task<ActionResult> GetEquipmentsUsingTemplate(string TSN)
        {
            try
            {
                var template = await _db.Template_OneDeviceOneCard.FindAsync(TSN)
                    ?? throw new MyCusResException("查無資料!");

                //List<EquipmentInfoDetailModel> result = new List<EquipmentInfoDetailModel>();
                //foreach (var e in template.EquipmentInfo)
                //    result.Add(
                //        await _eMgmtService.GetEquipmentInfoAsync<EquipmentInfoDetailModel>(e.ESN));

                var result = await Task.WhenAll(template.EquipmentInfo.Select(async e =>
                    await _eMgmtService.GetEquipmentInfoAsync<EquipmentInfoDetailModel>(e.ESN)));

                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Content($"</br>{ex.Message}", "application/json; charset=utf-8");
            }
            catch (Exception)
            {
                return Content("</br>系統異常!", "application/json; charset=utf-8");
            }
        }
        #endregion

        #region 一機一卡模板 詳情
        public ActionResult Detail(string id)
		{
            ViewBag.id = id;
            return View();
		}

        public async Task<ActionResult> ReadBody(string id)
        {
            try
            {
                // 獲取一機一卡詳情
                DeviceCardDetailViewModel deviceCard = await _dCardService.GetOneDeviceOneCardAsync<DeviceCardDetailViewModel>(id);
                // 獲取增設基本資料欄位
                deviceCard.AddItemList = await _dCardService.GetAddFieldListAsync(id);
                // 獲取保養項目
                deviceCard.MaintainItemList = await _dCardService.GetMaintainItemListAsync(id);
                // 獲取檢查項目
                deviceCard.CheckItemList = await _dCardService.GetCheckItemDetailListAsync(id);
                // 獲取填報項目名稱/單位
                deviceCard.ReportItemList = await _dCardService.GetReportItemDetailListAsync(id);

                return Content(JsonConvert.SerializeObject(deviceCard), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Content($"</br>{ex.Message}", "application/json; charset=utf-8");
            }
            catch (Exception)
            {
                return Content("</br>系統異常!", "application/json; charset=utf-8");
            }
        }
        #endregion

        #region 一機一卡模板 刪除
        public ActionResult Delete(string id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDeviceCard(string id)
        {
            try
            {
                var template = await _db.Template_OneDeviceOneCard.FindAsync(id);

                // 刪除增設基本資料欄位 : Equipment_AddField
                // 刪除關聯的 Equipment_AddFieldValue
                _dCardService.DeleteAddFieldList(new DeleteAddFieldList(template));

                // 刪除保養項目設定 : Equipment_MaintainItem
                // 刪除相關待派工及待執行的定期保養單 : Equipment_MaintenanceForm /Equipment_MaintenanceFormMember
                // 刪除關聯的 Equipment_MaintainItemValue
                _dCardService.DeleteMaintainItemList(new DeleteMaintainItemList(template));

                // 刪除檢查項目 : Template_CheckItem
                _dCardService.DeleteCheckItemList(new DeleteCheckItemList(template));

                // 刪除填報項目 : Template_ReportingItem
                _dCardService.DeleteReportItemList(new DeleteReportItemList(template));

                // 刪除一機一卡模板
                // 刪除模板與設備的關聯
                // 刪除使用該模板之設備待執行工單 TODO
                // 刪除使用該模板之巡檢預設順序 TODO
                await _dCardService.DeleteOneDeviceOneCardAsync(template);

                await _db.SaveChangesAsync();

                return Content("Succeed");
            }
            catch (MyCusResException ex)
            {
                return Content($"</br>{ex.Message}", "application/json; charset=utf-8");
            }
            catch (Exception)
            {
                return Content("</br>系統異常!", "application/json; charset=utf-8");
            }
        }
        #endregion

        // 覆寫 Dispose 方法來釋放資源
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db?.Dispose();  // _db釋放
            }
            base.Dispose(disposing);  // 呼叫父類別的Dispose方法
        }
    }
}