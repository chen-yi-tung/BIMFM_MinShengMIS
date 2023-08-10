using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class MeetingMinutes_ManagementController : Controller
	{
		// GET: MeetingMinutes_Management

		#region 會議記錄管理
		public ActionResult Management()
		{
			return View();
		}
		#endregion

		#region 新增會議記錄
		public ActionResult Create()
		{
			return View();
		}
		#endregion

		#region 編輯會議記錄
		public ActionResult Edit()
		{
			return View();
		}
		#endregion

		#region 會議記錄詳情
		public ActionResult Read()
		{
			return View();
		}
		#endregion

		#region 刪除會議記錄
		public ActionResult Delete()
		{
			return View();
		}
		#endregion
	}
}