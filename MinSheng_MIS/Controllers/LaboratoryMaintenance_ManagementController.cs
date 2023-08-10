using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class LaboratoryMaintenance_ManagementController : Controller
	{
		// GET: LaboratoryMaintenance_Management
		#region 實驗室維護管理
		public ActionResult Management()
		{
			return View();
		}
		#endregion

		#region 新增實驗室維護管理
		public ActionResult Create()
		{
			return View();
		}
		#endregion

		#region 編輯實驗室維護管理
		public ActionResult Edit()
		{
			return View();
		}
		#endregion

		#region 實驗室維護管理詳情
		public ActionResult Read()
		{
			return View();
		}
		#endregion
	}
}