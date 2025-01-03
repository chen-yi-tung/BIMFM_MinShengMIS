using MinSheng_MIS.Attributes;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.Mvc;
using static MinSheng_MIS.Services.UniParams;

namespace MinSheng_MIS.Controllers
{
    public class PlanManagementController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly PlanManagementService _inspectionPlanService;

        public PlanManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _inspectionPlanService = new PlanManagementService(_db);
        }

        #region 工單管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增工單
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// 新增工單
        /// </summary>
        /// <param name="data">使用者input</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateInspectionPlan(InspectionPlanCreateViewModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this, applyFormat: true);  // Data Annotation未通過

                // 建立 InspectionPlan
                string ipsn = await _inspectionPlanService.CreateInspectionPlanAsync(data);

                // 建立 InspectionPlan_Time
                await _inspectionPlanService.CreateInspectionPlanContentAsync(new InspectionPlanTimeModifiableListInstance(ipsn, data));

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

        #region 編輯工單
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }
        #endregion

        #region 工單詳情
        public ActionResult Detail(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public async Task<ActionResult> ReadBody(string id)
        {
            try
            {
                // 獲取工單
                var plan = await _inspectionPlanService.GetInspectionPlanAsync<InspectionPlanDetailViewModel>(id);
                // 獲取巡檢路線及時段
                foreach (var inspection in _inspectionPlanService.GetInspectionPlanTime(id))
                {
                    // 獲取巡檢紀錄
                    if (inspection.InspectionState != InspectionState.ToDo.GetLabel())
                    {
                        // 獲取巡檢路線設備
                        foreach (var item in await _inspectionPlanService.GetInspectionPlanEquipmentAsync(inspection.IPTSN))
                        {
                            // 獲取巡檢路線設備檢查項目
                            item.CheckItems = _inspectionPlanService.GetInspectionPlanCheckItems(item.IPESN);
                            // 獲取巡檢路線設備填報項目
                            item.RportItems = _inspectionPlanService.GetInspectionPlanRportItems(item.IPESN);

                            inspection.Equipments.Add(item);
                        }
                    }
                    plan.Inspections.Add(inspection as InspectionPlanContentDetail 
                        ?? throw new InvalidCastException("Invalid inspection type."));
                }

                return Content(JsonConvert.SerializeObject(new JsonResService<InspectionPlanDetailViewModel>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = plan,
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

        #region 工單 刪除
        public ActionResult Delete(string id)
        {
            ViewBag.id = id;
            return View();
        }
        /// <summary>
        /// 刪除工單
        /// </summary>
        /// <param name="data">使用者input</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteInspectionPlan(string IPSN)
        {
            JsonResService<string> result = new JsonResService<string>();
            try
            {
                // 刪除工單
                result = _inspectionPlanService.DeleteInspectionPlan(IPSN);
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = $"</br>{ex.Message}";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "</br>系統異常！";
                return Content(JsonConvert.SerializeObject(result), "application/json");
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