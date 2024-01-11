using MinSheng_MIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class WarningMessage_ManagementController : Controller
	{
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 警示訊息管理
        public ActionResult Management()
		{
			return View();
		}
        #endregion
        public ActionResult Edit()
		{
			return View();
		}
		public ActionResult Read()
		{
			return View();
		}
	}
}