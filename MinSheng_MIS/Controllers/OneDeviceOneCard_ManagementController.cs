using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this, applyFormat:true);  // Data Annotation未通過

                // 建立 Template_OneDeviceOneCard
                string tsn = await _dCardService.CreateOneDeviceOneCardAsync(data);
                if (data.Frequency.HasValue && data.CheckItemList?.Any() != true && data.ReportItemList?.Any() != true)
                    throw new MyCusResException("請至少填入一筆檢查項目或填報項目！");

                // 建立 Template_AddField (非必填)
                if (data.AddItemList?.Any() == true)
                    _dCardService.CreateAddFieldList(new AddFieldModifiableListInstance(tsn, data));

                // 建立 Template_MaintainItemSetting (非必填)
                if (data.MaintainItemList?.Any() == true)
                    _dCardService.CreateMaintainItemList(new MaintainItemModifiableListInstance(tsn, data));

                // 建立 Template_CheckItem (非必填)
                if (data.CheckItemList?.Any() == true)
                    _dCardService.CreateCheckItemList(new CheckItemModifiableListInstance(tsn, data));

                // 建立 Template_ReportingItem (非必填)
                if (data.ReportItemList?.Any() == true)
                    _dCardService.CreateReportItemList(new ReportItemModifiableListInstance(tsn, data));

                await _db.SaveChangesAsync();

                return Content(JsonConvert.SerializeObject(new JsonResService<string>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = null,
                }), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
            }
        }
        #endregion

        #region 一機一卡模板 編輯
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
                    ?? throw new MyCusResException("查無資料！");

                var result = await Task.WhenAll(template.EquipmentInfo.Select(async e =>
                    await _eMgmtService.GetEquipmentInfoAsync<EquipmentInfoDetailModel>(e.ESN)));

                return Content(JsonConvert.SerializeObject(new JsonResService<EquipmentInfoDetailModel[]>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = result,
                }), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditDeviceCard(DeviceCardEditViewModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this, applyFormat: true);  // Data Annotation未通過

                using (var trans = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // 更新一機一卡模板
                    await _dCardService.UpdateOneDeviceOneCardAsync(data);

                    // 更新增設基本資料欄位
                    await _dCardService.UpdateAddFieldListAsync(data);

                    // 更新保養項目
                    await _dCardService.UpdateMaintainItemListAsync(data);

                    // 更新檢查項目
                    await _dCardService.UpdateCheckItemListAsync(data);

                    // 更新填報項目
                    await _dCardService.UpdateReportItemListAsync(data);

                    await _db.SaveChangesAsync();

                    // 提交交易
                    trans.Complete();
                }

                return Content(JsonConvert.SerializeObject(new JsonResService<string>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = null,
                }), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
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

                return Content(JsonConvert.SerializeObject(new JsonResService<DeviceCardDetailViewModel>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = deviceCard,
                }), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
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

                return Content(JsonConvert.SerializeObject(new JsonResService<string>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = null,
                }), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
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