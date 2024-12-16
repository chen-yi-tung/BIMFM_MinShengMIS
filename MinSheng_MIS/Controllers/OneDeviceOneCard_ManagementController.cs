using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class OneDeviceOneCard_ManagementController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly OneDeviceOneCard_ManagementService _dCardService;

        public OneDeviceOneCard_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _dCardService = new OneDeviceOneCard_ManagementService(_db);
        }

        #region 一機一卡模板管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 一機一卡模板
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateDeviceCard(DeviceCardCreateModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 建立 Template_OneDeviceOneCard
                string tsn = await _dCardService.CreateDeviceCardAsync(data);
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
                return Content(ex.Message, "application/json; charset=utf-8");
            }
            catch (Exception)
            {
                return Content("系統異常!", "application/json; charset=utf-8");
            }
        }
        #endregion

        #region 編輯 一機一卡模板
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 一機一卡模板 詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion

        #region 一機一卡模板 刪除
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