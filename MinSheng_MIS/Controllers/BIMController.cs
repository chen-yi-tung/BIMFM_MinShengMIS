using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinSheng_MIS.Controllers
{
	public class BIMController : Controller
	{
		Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
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

		[HttpGet]
		public ActionResult FloorInfo(string fsn)
		{
			var data = db.Floor_Info.FirstOrDefault(x=>x.FSN == fsn);
			JObject result = new JObject();
			result["ViewName"] = data.ViewName;
			result["FSN"] = data.FSN;
			result["Floor"] = data.FloorName;
			result["Area"] = data.AreaInfo.Area;
			result["ASN"] = data.AreaInfo.ASN;
			return Content(JsonConvert.SerializeObject(result), "application/json");
		}
	}
}