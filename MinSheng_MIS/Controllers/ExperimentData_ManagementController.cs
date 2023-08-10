using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class ExperimentData_ManagementController : Controller
	{
		// GET: ExperimentData_Management
		#region 實驗數據管理
		public ActionResult Management()
		{
			return View();
		}
		#endregion

		#region 新增實驗數據紀錄
		public ActionResult Create()
		{
			return View();
		}
		#endregion

		#region 編輯實驗數據紀錄
		public ActionResult Edit()
		{
			return View();
		}
		#endregion

		#region 實驗數據紀錄詳情
		public ActionResult Read()
		{
			return View();
		}
		#endregion
	}
}