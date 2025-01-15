using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Transactions;
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
                data.SetPlanPathSN(await _samplePathService.CreateSamplePathAsync(data));

                // 建立 InspectionDefaultOrder
                _samplePathService.CreateInspectionDefaultOrders(data);

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

        /// <summary>
        /// 編輯巡檢路線模板
        /// </summary>
        /// <param name="data">使用者input</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> EditSamplePath(SamplePathEditViewModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this, applyFormat: true);  // Data Annotation未通過

                using (var trans = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // 更新 InspectionPathSample
                    await _samplePathService.EditSamplePathAsync(data);
                    // 更新 InspectionDefaultOrder
                    await _samplePathService.EditInspectionDefaultOrdersAsync(data);

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

        #region 巡檢路線模板 詳情
        public ActionResult Detail()
        {
            return View();
        }

        public ActionResult ReadBody(string id)
        {
            try
            {
                // 獲取巡檢路線模板詳情
                SamplePathDetailViewModel sample = _samplePathService.GetSamplePath<SamplePathDetailViewModel>(id);
                // 獲取路線RFID順序
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

        /// <summary>
        /// 刪除巡檢路線模板
        /// </summary>
        /// <param name="id">巡檢路線模板編號</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> DeleteSamplePath(string id)
        {
            try
            {
                var path = await _db.InspectionPathSample.SingleOrDefaultAsync(x => x.PlanPathSN == id)
                    ?? throw new MyCusResException("巡檢路線模板不存在！");

                using (var trans = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _samplePathService.DeleteInspectionPathSampleAsync(path);

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

        #region 新增巡檢設備DataGrid
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