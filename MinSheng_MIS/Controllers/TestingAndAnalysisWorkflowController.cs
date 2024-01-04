using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
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

namespace MinSheng_MIS.Controllers
{
	public class TestingAndAnalysisWorkflowController : Controller
	{
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: TestingAndAnalysisWorkflow
        #region 採驗分析流程建立
        public ActionResult Management()
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
			for (int i = 0; i < ta_workflow.LabelName.Count; i++)
			{
                if (string.IsNullOrEmpty(ta_workflow.LabelName[i].Trim())) continue;
                var label = new TestingAndAnalysis_LabelName
                {
                    LNSN = workflow.TAWSN + "_" + (i+1).ToString().PadLeft(3, '0'),
					TAWSN = workflow.TAWSN,
					LabelName = ta_workflow.LabelName[i]
                };
				db.TestingAndAnalysis_LabelName.Add(label);
            }
            // 新增採驗分析_實驗數據
            for (int i = 0; i < ta_workflow.DataName.Count; i++)
            {
                if (string.IsNullOrEmpty(ta_workflow.DataName[i].Trim())) continue;
                var data = new TestingAndAnalysis_DataName
                {
                    DNSN = workflow.TAWSN + "_" + (i + 1).ToString().PadLeft(3, '0'),
                    TAWSN = workflow.TAWSN,
                    DataName = ta_workflow.DataName[i]
                };
                db.TestingAndAnalysis_DataName.Add(data);
            }
            // [實驗採樣分析流程檔案]檔案處理，目前只提供單個檔案上傳
            string folderpath = Server.MapPath("~/Files/TestingAndAnalysisWorkflow/");
            if (ta_workflow.WorkflowFile != null && ta_workflow.WorkflowFile.ContentLength > 0) // 上傳
            {
                var File = ta_workflow.WorkflowFile;
                string extension = Path.GetExtension(File.FileName); // 檔案副檔名
                if (ComFunc.IsConformedForDocument(File.ContentType, extension) || ComFunc.IsConformedForImage(File.ContentType, extension)) // 檔案白名單檢查
                {
                    // 檔案上傳
                    if (!ComFunc.UploadFile(File, folderpath, workflow.TAWSN))
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "檔案上傳過程出錯!");
                    workflow.WorkflowFile = workflow.TAWSN + extension;
                    db.TestingAndAnalysisWorkflow.AddOrUpdate(workflow);
                }
                else
                    return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType, "非系統可接受的檔案格式!");
            }
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 編輯採樣分析流程
        public ActionResult Edit()
		{
			return View();
		}
		#endregion

		#region 採樣分析流程詳情
		public ActionResult Read()
		{
			return View();
		}
		#endregion
	}
}