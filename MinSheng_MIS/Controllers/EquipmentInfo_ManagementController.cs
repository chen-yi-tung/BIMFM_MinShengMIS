using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static MinSheng_MIS.Services.Helper;

namespace MinSheng_MIS.Controllers
{
    public class EquipmentInfo_ManagementController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly EquipmentInfo_ManagementService _eMgmtService;
        private readonly RFIDService _rfidService;

        public EquipmentInfo_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _eMgmtService = new EquipmentInfo_ManagementService(_db);
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

        [HttpPost]
        public async Task<ActionResult> CreateEquipment(EquipmentInfoCreateModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

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
                        //await _eMgmtService.CreateEquipmentAdditionalFieldsValue(data.ConvertToUpdateAddFieldValue(esn));

                    // 建立 Equipment_MaintainItemValue (非必填)
                    if (data.MaintainItemList != null && data.MaintainItemList.Any())
                        await _eMgmtService.CreateEquipmentMaintainItemsValue(data.ConvertToUpdateMaintainItemValue(esn));
                }

                await _db.SaveChangesAsync();

                return Content("Succeed");
            }
            catch (MyCusResException ex)
            {
                return Content(ex.Message, "application/json; charset=utf-8");
            }
            catch (Exception)
            {
                return Content("系統異常!", "application/json; charset=utf-8");
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
        public ActionResult Detail()
        {
            return View();
        }

        //public async Task<ActionResult> Read_Data(string id)
        //{
        //    var data = await _db.EquipmentInfo.FirstOrDefaultAsync(x => x.ESN == id)
        //        ?? throw new HttpException(400, "EDRSN is Undefined.");
            

        //    ED_ViewModel model = new ED_ViewModel
        //    {
        //        ExperimentType = data.TestingAndAnalysisWorkflow?.ExperimentType,
        //        ExperimentName = data.TestingAndAnalysisWorkflow?.ExperimentName,
        //        TAWSN = data.TAWSN,
        //        EDate = data.EDate.ToString("yyyy-MM-dd"),
        //        UploadUserName = db.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == data.UploadUserName)?.Result.MyName,
        //        UploadDateTime = data.UploadDateTime.ToString("yyyy/MM/dd"),
        //        FilePath = !string.IsNullOrEmpty(data.EDFile) ? ComFunc.UrlMaker(folderPath, data.EDFile) : null,
        //        ExperimentalDataItem = data.ExperimentalData.Select(x => new ED_Info { DataName = x.DataName, Data = x.Data }).ToList()
        //    };
        //    return Content(JsonConvert.SerializeObject(model), "application/json");
        //}
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