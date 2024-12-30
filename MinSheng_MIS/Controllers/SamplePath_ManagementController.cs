using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class SamplePath_ManagementController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly SamplePath_ManagementService _samplePathService;

        public SamplePath_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _samplePathService = new SamplePath_ManagementService(_db);
        }

        #region 巡檢路線模板管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 巡檢路線模板
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// 新增巡檢設備Grid
        /// </summary>
        /// <param name="data">查詢條件及Grid參數</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddEquipmentRFIDsGrid(EquipmentRFIDSearchParamViewModel data)
        {
            try
            {
                var grid = _samplePathService.GetJsonForGrid_EquipmentRFIDs(data);
                return Content(JsonConvert.SerializeObject(new JsonResService<GridResult<InspectionRFIDsViewModel>>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = grid,
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

        /// <summary>
        /// 新增巡檢路線模板
        /// </summary>
        /// <param name="data">使用者input</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateSamplePath(SamplePathCreateViewModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this, applyFormat: true);  // Data Annotation未通過

                // 建立 InspectionPathSample
                string planPathSN = await _samplePathService.CreateSamplePathAsync(data);

                // 建立 InspectionDefaultOrder
                _samplePathService.CreateInspectionDefaultOrders(new DefaultOrderModifiableListInstance(planPathSN, data));

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

        #region 編輯 巡檢路線模板
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 巡檢路線模板 詳情
        public ActionResult Detail()
        {
            return View();
        }

        public async Task<ActionResult> ReadBody(string id)
        {
            try
            {
                // 獲取一機一卡詳情
                SamplePathDetailViewModel sample = await _samplePathService.GetSamplePathAsync<SamplePathDetailViewModel>(id);
                // 獲取增設基本資料欄位
                sample.Equipments = _samplePathService.GetDefaultOrderRFIDInfoList(id);

                return Content(JsonConvert.SerializeObject(new JsonResService<SamplePathDetailViewModel>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = sample,
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

        #region 巡檢路線模板 刪除
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