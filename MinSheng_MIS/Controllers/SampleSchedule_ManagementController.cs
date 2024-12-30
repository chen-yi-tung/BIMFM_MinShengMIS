using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class SampleSchedule_ManagementController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly SampleSchedule_ManagementService _sampleScheduleService;

        public SampleSchedule_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _sampleScheduleService = new SampleSchedule_ManagementService(_db);
        }

        #region 每日巡檢時程模板管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 每日巡檢時程模板
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// 新增每日巡檢時程安排模板
        /// </summary>
        /// <param name="data">使用者input</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateSampleSchedule(SampleScheduleCreateViewModel data)
        {
            try
            {
                // Data Annotation
                if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this, applyFormat: true);  // Data Annotation未通過

                // 建立 DailyInspectionSample
                string dailyTemplateSN = await _sampleScheduleService.CreateInspectionSampleAsync(data);

                // 建立 DailyInspectionSampleContent
                _sampleScheduleService.CreateInspectionSampleContent(new SampleContentModifiableListInstance(dailyTemplateSN, data));

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

        #region 編輯 每日巡檢時程模板
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 每日巡檢時程模板 詳情
        public ActionResult Detail()
        {
            return View();
        }

        public async Task<ActionResult> ReadBody(string id)
        {
            try
            {
                // 獲取一機一卡詳情
                var sample = await _sampleScheduleService.GetInspectionSampleAsync<SampleScheduleDetailViewModel>(id);
                // 獲取增設基本資料欄位
                sample.Contents = _sampleScheduleService.GetSampleScheduleContentList(id);

                return Content(JsonConvert.SerializeObject(new JsonResService<SampleScheduleDetailViewModel>
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

        #region 每日巡檢時程模板 刪除
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