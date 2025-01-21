using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static System.Net.Mime.MediaTypeNames;

namespace MinSheng_MIS.Controllers
{
	public class TestingAndAnalysisWorkflowController : Controller
	{
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        static readonly string folderPath = "Files/TestingAndAnalysisWorkflow";
        // GET: TestingAndAnalysisWorkflow
        #region 採驗分析流程建立
        public ActionResult Index()
		{
			return View();
		}
		#endregion

		#region 新增採樣分析流程
		public ActionResult Create()
		{
			return View();
		}

        [HttpPost]
        public async Task<ActionResult> CreateTestingAndAnalysisWorkflow(TA_Workflow ta_workflow)
		{
            ModelState.Remove("TAWSN");
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過
            else if (ta_workflow.LabelName.Where(x => x.Trim().Length > 200).Count() > 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "實驗標籤名稱 的長度最多200個字元。");
            else if (ta_workflow.DataName.Where(x => x.Trim().Length > 200).Count() > 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "實驗數據欄位名稱 的長度最多200個字元。");

            DateTime now = DateTime.Now;
            // 新增採驗分析流程
            var count = await db.TestingAndAnalysisWorkflow.Where(x => DbFunctions.TruncateTime(x.UploadDateTime) == now.Date).CountAsync() + 1;  // 採驗分析流程流水碼
			var workflow = new TestingAndAnalysisWorkflow
			{
				TAWSN = now.ToString("yyMMdd") + count.ToString().PadLeft(3, '0'),
				ExperimentType = ta_workflow.ExperimentType,
				ExperimentName = ta_workflow.ExperimentName,
                UploadUserName = User.Identity.Name,
				UploadDateTime = now,
                WorkflowFile = "undefined"
            };
			db.TestingAndAnalysisWorkflow.Add(workflow);
            // 新增採驗分析_實驗標籤
            ICollection<TestingAndAnalysis_LabelName> labelList = AddOrUpdateList<TestingAndAnalysis_LabelName>(ta_workflow.LabelName, workflow.TAWSN);
            db.TestingAndAnalysis_LabelName.AddRange(labelList);
            // 新增採驗分析_實驗數據
            ICollection<TestingAndAnalysis_DataName> dataList = AddOrUpdateList<TestingAndAnalysis_DataName>(ta_workflow.DataName, workflow.TAWSN);
            db.TestingAndAnalysis_DataName.AddRange(dataList);
            // [實驗採樣分析流程檔案]檔案處理，目前只提供單個檔案上傳
            if (ta_workflow.WorkflowFile != null && ta_workflow.WorkflowFile.ContentLength > 0) // 上傳
            {
                var File = ta_workflow.WorkflowFile;
                string extension = Path.GetExtension(File.FileName); // 檔案副檔名
                if (ComFunc.IsConformedForDocument(File.ContentType, extension)
                    || ComFunc.IsConformedForPdf(File.ContentType, extension)
                    || ComFunc.IsConformedForImage(File.ContentType, extension)) // 檔案白名單檢查
                {
                    // 檔案上傳
                    if (!ComFunc.UploadFile(File, Server.MapPath($"~/{folderPath}/"), workflow.TAWSN))
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "檔案上傳過程出錯！");
                    workflow.WorkflowFile = workflow.TAWSN + extension;
                    db.TestingAndAnalysisWorkflow.AddOrUpdate(workflow);
                }
                else
                    //return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType, "非系統可接受的檔案格式！");
                    return Content("<br>非系統可接受的檔案格式!<br>僅支援上傳圖片、Word或PDF！", "application/json; charset=utf-8");
            }
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 編輯採樣分析流程
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditTestingAndAnalysisWorkflow(TA_Workflow ta_workflow)
        {
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

            var workflow = await db.TestingAndAnalysisWorkflow.FirstOrDefaultAsync(x => x.TAWSN == ta_workflow.TAWSN);
            if (workflow == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "TAWSN is Undefined.");
            else if (ta_workflow.LabelName.Where(x => x.Trim().Length > 200).Count() > 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "實驗標籤名稱 的長度最多200個字元。");
            else if (ta_workflow.DataName.Where(x => x.Trim().Length > 200).Count() > 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "實驗數據欄位名稱 的長度最多200個字元。");

            // 編輯採樣分析流程
            workflow.ExperimentType = ta_workflow.ExperimentType;
            workflow.ExperimentName = ta_workflow.ExperimentName;
            // 編輯實驗標籤
            db.TestingAndAnalysis_LabelName.RemoveRange(workflow.TestingAndAnalysis_LabelName);
            ICollection<TestingAndAnalysis_LabelName> ta_labels = AddOrUpdateList<TestingAndAnalysis_LabelName>(ta_workflow.LabelName, workflow.TAWSN);
            workflow.TestingAndAnalysis_LabelName = ta_labels;
            // 編輯實驗數據
            db.TestingAndAnalysis_DataName.RemoveRange(workflow.TestingAndAnalysis_DataName);
            ICollection<TestingAndAnalysis_DataName> ta_datas = AddOrUpdateList<TestingAndAnalysis_DataName>(ta_workflow.DataName, workflow.TAWSN);
            workflow.TestingAndAnalysis_DataName = ta_datas;
            // [實驗採樣分析流程檔案]檔案處理，目前只提供單個檔案上傳且刪除
            if (ta_workflow.WorkflowFile != null && ta_workflow.WorkflowFile.ContentLength > 0) // 上傳
            {
                var newFile = ta_workflow.WorkflowFile;
                string extension = Path.GetExtension(newFile.FileName); // 檔案副檔名
                if (ComFunc.IsConformedForDocument(newFile.ContentType, extension)
                    || ComFunc.IsConformedForPdf(newFile.ContentType, extension)
                    || ComFunc.IsConformedForImage(newFile.ContentType, extension)) // 檔案白名單檢查
                {
                    // 舊檔案刪除
                    ComFunc.DeleteFile(Server.MapPath($"~/{folderPath}/"), workflow.WorkflowFile, null);
                    // 檔案上傳
                    if (!ComFunc.UploadFile(newFile, Server.MapPath($"~/{folderPath}/"), workflow.TAWSN))
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "檔案上傳過程出錯!(舊檔案已刪除)");
                    workflow.WorkflowFile = workflow.TAWSN + extension;
                }
                else
                    //return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType, "非系統可接受的檔案格式！");
                    return Content("<br>非系統可接受的檔案格式!<br>僅支援上傳圖片、Word或PDF！", "application/json; charset=utf-8");
            }

            db.TestingAndAnalysisWorkflow.AddOrUpdate(workflow);
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 採樣分析流程詳情
        public ActionResult Detail(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public async Task<ActionResult> Read_Data(string id)
        {
            var workflow = await db.TestingAndAnalysisWorkflow.FirstOrDefaultAsync(x => x.TAWSN == id);
            if (workflow == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "TAWSN is Undefined.");

            TA_Workflow_ViewModel model = new TA_Workflow_ViewModel
            {
                TAWSN = workflow.TAWSN,
                ExperimentType = workflow.ExperimentType,
                ExperimentName = workflow.ExperimentName,
                UploadUserName = db.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == workflow.UploadUserName)?.Result.MyName,
                UploadDateTime = workflow.UploadDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                FilePath = !string.IsNullOrEmpty(workflow.WorkflowFile) ? ComFunc.UrlMaker(folderPath, workflow.WorkflowFile) : null,
                LabelNames = workflow.TestingAndAnalysis_LabelName.Select(x => new TA_Label { LNSN = x.LNSN, LabelName = x.LabelName})?.ToList(),
                DataNames = workflow.TestingAndAnalysis_DataName.Select(x => new TA_Data { DNSN = x.DNSN, DataName = x.DataName})?.ToList()
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
        #endregion

        #region 實驗室標籤管理-獲取流程實驗標籤名稱
        [HttpGet]
        public ActionResult TestingAndAnalysis_LabelName(string TAWSN)
        {
            List<JObject> list = new List<JObject>();
            var labels = db.TestingAndAnalysisWorkflow.FirstOrDefault(x => x.TAWSN == TAWSN)?.TestingAndAnalysis_LabelName;
            foreach (var item in labels)
            {
                JObject jo = new JObject
                {
                    { "LabelName", item.LabelName },
                };
                list.Add(jo);
            }
            return Content(JsonConvert.SerializeObject(list), "application/json");
        }
        #endregion

        #region 實驗室標籤管理-獲取流程實驗數據欄位名稱
        [HttpGet]
        public ActionResult TestingAndAnalysis_DataName(string TAWSN)
        {
            List<JObject> list = new List<JObject>();
            var labels = db.TestingAndAnalysisWorkflow.FirstOrDefault(x => x.TAWSN == TAWSN)?.TestingAndAnalysis_DataName;
            foreach (var item in labels)
            {
                JObject jo = new JObject
                {
                    { "DataName", item.DataName },
                };
                list.Add(jo);
            }
            return Content(JsonConvert.SerializeObject(list), "application/json");
        }
        #endregion

        #region Helper
        private static ICollection<T> AddOrUpdateList<T>(List<string> list, string TAWSN) where T : class, new()
        {
            Type typeOfT = typeof(T);
            ICollection<T> result = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                if (string.IsNullOrEmpty(list[i].Trim())) continue;
                dynamic newItem = new T(); // 使用 dynamic 以避免編譯錯誤

                if (typeOfT == typeof(TestingAndAnalysis_LabelName))
                {
                    newItem.LNSN = TAWSN + "_" + (i + 1).ToString().PadLeft(3, '0');
                    newItem.LabelName = list[i];
                }
                else
                {
                    newItem.DNSN = TAWSN + "_" + (i + 1).ToString().PadLeft(3, '0');
                    newItem.DataName = list[i];
                }
                newItem.TAWSN = TAWSN;

                result.Add((T)newItem);
            }
            return result;
        }
        #endregion
    }
}