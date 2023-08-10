using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class InspectionAnalysisWorkflowController : Controller
	{
		// GET: InspectionAnalysisWorkflow
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