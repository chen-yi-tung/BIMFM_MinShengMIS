using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class LaboratoryLabel_ManagementController : Controller
	{
		// GET: LaboratoryLabel_Management
		#region 實驗室標籤管理
		public ActionResult Management()
		{
			return View();
		}
		#endregion

		#region 新增實驗標籤
		public ActionResult Create()
		{
			return View();
		}
		#endregion

		#region 編輯實驗標籤
		public ActionResult Edit()
		{
			return View();
		}
		#endregion

		#region 實驗標籤詳情
		public ActionResult Read()
		{
			return View();
		}
		#endregion
	}
}