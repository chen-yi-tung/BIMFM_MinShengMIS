using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class MonthlyReport_ManagementController : Controller
	{
		// GET: MonthlyReport_Management

		#region 月報管理
		public ActionResult Management()
		{
			return View();
		}
		#endregion

		#region 新增月報
		public ActionResult Create()
		{
			return View();
		}
		#endregion

		#region 編輯月報
		public ActionResult Edit()
		{
			return View();
		}
		#endregion

		#region 月報詳情
		public ActionResult Read()
		{
			return View();
		}
		#endregion

		#region 刪除月報
		public ActionResult Delete()
		{
			return View();
		}
		#endregion
	}
}