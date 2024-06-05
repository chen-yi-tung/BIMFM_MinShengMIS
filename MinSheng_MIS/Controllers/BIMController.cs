using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class BIMController : Controller
	{
		// GET: BIM
		public ActionResult FloorModelView(string id)
		{
			ViewBag.id = id;
			return View();
		}

		public ActionResult ModelView(string id)
		{
			ViewBag.id = id;
			return View();
		}
	}
}