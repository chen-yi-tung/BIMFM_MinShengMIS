using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentInfo_ManagementController : Controller
    {
        private readonly string _photoPath = "Files/EquipmentInfo";
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly EquipmentInfo_ManagementService _eMgmtService;
        private readonly OneDeviceOneCard_ManagementService _dCardService;
        private readonly RFIDService _rfidService;

        public EquipmentInfo_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _eMgmtService = new EquipmentInfo_ManagementService(_db);
            _dCardService = new OneDeviceOneCard_ManagementService(_db);
            _rfidService = new RFIDService(_db);
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
                {
                    foreach (var item in data.RFIDList)
                        await _rfidService.AddEquipRFIDAsync(item, esn); // 新增單個RFID
                }

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

                // 檔案上傳
                if (!ComFunc.UploadFile(data.EPhoto, Server.MapPath($"~/{_photoPath}/"), esn))
                    throw new MyCusResException("檔案上傳過程出錯！");

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
        public ActionResult Edit()
        {
            return View();
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