using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

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
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 工單詳情
        public ActionResult Detail()
        {
            return View();
        }
        #endregion

        #region 工單 刪除
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}