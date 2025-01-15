using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using static MinSheng_MIS.Services.UniParams;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentInfo_ManagementController : Controller
    {
        
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly EquipmentInfo_ManagementService _eMgmtService;
        private readonly OneDeviceOneCard_ManagementService _dCardService;

        public EquipmentInfo_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _eMgmtService = new EquipmentInfo_ManagementService(_db);
            _dCardService = new OneDeviceOneCard_ManagementService(_db);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            _eMgmtService.SetServer(Server);  // 將 Server 傳遞給服務層
            _dCardService.SetServer(Server);  // 將 Server 傳遞給服務層
        }

        #region 資產管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 設備
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// 新增設備API
        /// </summary>
        /// <param name="data">Input</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateEquipment(EquipmentInfoCreateViewModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this, applyFormat:true);  // Data Annotation未通過

                // 建立 EquipmentInfo
                string esn = await _eMgmtService.CreateEquipmentInfoAsync(data);

                // 建立 RFID
                if (data.RFIDList != null && data.RFIDList.Any())
                    await _eMgmtService.CreateRFIDAsync(data.ConvertToUpdateRFID(esn));

                // 建立一機一卡模板資料 (非必填)
                if (data.TSN != null)
                {
                    // 建立 Equipment_AddFieldValue (非必填)
                    if (data.AddFieldList != null && data.AddFieldList.Any())
                        await _eMgmtService.CreateEquipmentAdditionalFieldsValue(data.ConvertToUpdateAddFieldValue(esn));

                    // 建立 Equipment_MaintainItemValue (非必填)
                    if (data.MaintainItemList != null && data.MaintainItemList.Any())
                        await _eMgmtService.CreateEquipmentMaintainItemsValue(data.ConvertToUpdateMaintainItemValue(esn));
                }

                // 照片上傳
                if (data.EPhoto?.ContentLength > 0)
                    _eMgmtService.UploadPhoto(data.EPhoto, esn);

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

        #region 編輯 設備
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditEquipment(EquipmentInfoEditViewModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this, applyFormat: true);  // Data Annotation未通過
                
                using (var trans = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // 更新設備資訊
                    await _eMgmtService.UpdateEquipmentInfoAsync(data);

                    // 更新RFID
                    await _eMgmtService.UpdateRFIDAsync(data.ConvertToUpdateRFID(data.ESN));

                    // 更新一機一卡模板資料
                    if (data.TSN != null)
                    {
                        // 更新 Equipment_AddFieldValue (非必填)
                        if (data.AddFieldList != null && data.AddFieldList.Any())
                            await _eMgmtService.UpdateEquipmentAdditionalFieldsValueAsync(data);

                        // 更新 Equipment_MaintainItemValue (非必填)
                        if (data.MaintainItemList != null && data.MaintainItemList.Any())
                            await _eMgmtService.UpdateEquipmentMaintainItemsValue(data);
                    }

                    // 照片刪除
                    if (string.IsNullOrEmpty(data.FileName))
                        _eMgmtService.DeletePhoto(data.ESN);

                    // 照片上傳
                    if (data.EPhoto?.ContentLength > 0)
                        _eMgmtService.UploadPhoto(data.EPhoto, data.ESN);

                    await _db.SaveChangesAsync();

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

        #region 設備 詳情
        public ActionResult Detail(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public async Task<ActionResult> ReadBody(string id)
        {
            try
            {
                // 獲取設備詳情
                var equipment = await _eMgmtService.GetEquipmentInfoAsync<EquipmentInfoDetailViewModel>(id);
                // 獲取增設基本資料欄位
                equipment.AddFieldList = _eMgmtService.GetAddFieldList(id);
                // 獲取保養項目
                equipment.MaintainItemList = _eMgmtService.GetMaintainItemList(id);
                // 獲取RFID
                equipment.RFIDList = await _eMgmtService.GetRFIDListAsync(id);

                if (!string.IsNullOrEmpty(equipment.TSN))
                {
                    // 獲取檢查項目
                    equipment.CheckItemList = await _dCardService.GetCheckItemDetailListAsync(equipment.TSN);
                    // 獲取填報項目名稱/單位
                    equipment.ReportItemList = await _dCardService.GetReportItemDetailListAsync(equipment.TSN);
                }

                return Content(JsonConvert.SerializeObject(new JsonResService<EquipmentInfoDetailViewModel>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = equipment,
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

        #region 刪除 設備
        public ActionResult Delete()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DisableEquipment(string id)
        {
            try
            {
                var equipment = await _db.EquipmentInfo.SingleOrDefaultAsync(x => x.ESN == id)
                        ?? throw new MyCusResException("設備不存在！");
                if (IsEnumEqualToStr(equipment.EState, EState.Repair))
                    throw new MyCusResException("設備報修中，不可停用！");
                if (IsEnumEqualToStr(equipment.EState, EState.Disable))
                    throw new MyCusResException("設備已停用！");

                using (var trans = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // EquipmentInfo : 停用設備資料, 停用模板
                    _eMgmtService.DisableEquipment(equipment);
                    // 刪除關聯的待派工 Equipment_MaintenanceForm
                    equipment.Equipment_MaintenanceForm
                        .Where(x => IsEnumEqualToStr(x.Status, MaintenanceFormStatus.ToAssign))
                        .ToList()
                        .ForEach(x => _db.Equipment_MaintenanceForm.Remove(x));
                    // 刪除關聯的待派工 EquipmentReportForm
                    equipment.EquipmentReportForm
                        .Where(x => IsEnumEqualToStr(x.ReportState, ReportFormStatus.ToAssign))
                        .ToList()
                        .ForEach(x => _db.EquipmentReportForm.Remove(x));

                    // 刪除 RFID 關聯的 InspectionPlan_RFIDOrder
                    _db.InspectionPlan_RFIDOrder.RemoveRange(equipment.RFID.SelectMany(x => x.InspectionPlan_RFIDOrder));
                    // 刪除 RFID 及 關聯的 InspectionDefaultOrder
                    await _eMgmtService.DeleteRFIDAsync(equipment.RFID.ToList());

                    // Equipment_AddFieldValue : 全部刪除
                    _eMgmtService.DeleteAddFieldValueList(
                        equipment.Equipment_AddFieldValue.Select(x => x.EAFVSN));

                    // Equipment_MaintainItemValue : 全部刪除
                    _eMgmtService.DeleteMaintainItemValueList(
                        equipment.Equipment_MaintainItemValue.Select(x => x.EMIVSN));

                    await _db.SaveChangesAsync();

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