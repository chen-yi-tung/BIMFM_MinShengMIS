using Microsoft.AspNet.Identity;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class WarningMessage_ManagementController : Controller
	{
		Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

		#region 警示訊息管理
		public ActionResult Index()
		{
			return View();
		}
        [HttpPost]
        public ActionResult AddWarningMessagen(WarningMessageCreateModel info)
        {
            WarningMessageService wms = new WarningMessageService();
            wms.AddWarningMessage(info, User.Identity.Name);
            return Content(JsonConvert.SerializeObject(new JObject { { "Succeed", true } }), "application/json");
        }
        #endregion

        #region 警示訊息填報
        public ActionResult Edit(string id)
		{
			ViewBag.id = id;
			return View();
		}
		[HttpPost]
		public ActionResult WarningMessageFillin(FillinInfo info)
		{
			WarningMessageService wms = new WarningMessageService();
			wms.AddWarningMessageFillinRecord(info, User.Identity.Name);
			return Content(JsonConvert.SerializeObject(new JObject { { "Succeed", true } }), "application/json");
		}
		#endregion

		#region 警示訊息詳情
		public ActionResult Detail(string id)
		{
			ViewBag.id = id;
			return View();
		}
		[HttpGet]
		public ActionResult GetWarningMessageInfo(string WMSN)
		{
			var messageinfo = db.WarningMessage.Find(WMSN);
			var WMTypedic = Surface.WMType(); //警示訊息事件等級對照
			var WMState = Surface.WMState(); //警示訊息事件處理狀況對照
			var FloorName = db.Floor_Info.Find(messageinfo.FSN).FloorName.ToString();
			var Area = db.Floor_Info.Find(messageinfo.FSN).AreaInfo.Area.ToString();

			//取得警示訊息資訊
			WarningMessageViewModel warningMessage = new WarningMessageViewModel();
			warningMessage.WMSN = WMSN;
			warningMessage.WMType = WMTypedic[messageinfo.WMType];
			warningMessage.WMState = WMState[messageinfo.WMState];
			warningMessage.TimeOfOccurrence = messageinfo.TimeOfOccurrence.ToString("yyyy-MM-dd HH:mm:ss");
			warningMessage.Location = Area + " " + FloorName;
			warningMessage.Message = messageinfo.Message;

			//取得填報紀錄
			var records = db.WarningMessageFillinRecord.Where(x => x.WMSN == WMSN).ToList();
			if (records.Count() > 0)
			{
				List<WarningMessageFillinRecordViewModel> rlist = new List<WarningMessageFillinRecordViewModel>();
				foreach (var record in records)
				{
					WarningMessageFillinRecordViewModel r = new WarningMessageFillinRecordViewModel();
					r.FillinDateTime = record.FillinDateTime.ToString("yyyy-MM-dd HH:mm:ss");
					r.MyName = db.AspNetUsers.Where(x => x.UserName == record.FillinUserName).FirstOrDefault().MyName.ToString();
					r.FillinState = WMState[record.FillinState];
					r.Memo = record.Memo;
					rlist.Add(r);
				}
				warningMessage.WarningMessageFillinRecord = rlist;
			}
			else
			{
				warningMessage.WarningMessageFillinRecord = null;
			}

			string result = JsonConvert.SerializeObject(warningMessage);
			return Content(result, "application/json");
		}
		#endregion

		#region 小鈴鐺警示訊息
		[AllowAnonymous]
		[HttpGet]
		public ActionResult BellMessageInfo()
		{
			JArray messages = new JArray();
			var messagelist = db.WarningMessage.Where(x=> x.WMState == "1"|| x.WMState == "2").OrderByDescending(x => x.TimeOfOccurrence).ToList();
			foreach(var m in messagelist)
			{
				JObject message = new JObject();
				message.Add("WMSN", m.WMSN);
				message.Add("Location", db.Floor_Info.Find(m.FSN).AreaInfo.Area.ToString() + db.Floor_Info.Find(m.FSN).FloorName.ToString());
				message.Add("Message", m.Message);
				message.Add("WMType", m.WMType);
				message.Add("WMState", m.WMState);
				message.Add("TimeOfOccurrence", m.TimeOfOccurrence.ToString("yyyy-MM-dd HH:mm:ss"));
				messages.Add(message);
			}

			return Content(JsonConvert.SerializeObject(messages), "application/json");
		}
		#endregion

		#region 小鈴鐺已讀列表 - 讀取
		[HttpGet]
		public ActionResult GetHaveReadMessage()
		{
			try
			{
				string folderPath = Server.MapPath("~/Files/HaveReadMessage");
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}

				string fileName = Server.MapPath($"~/Files/HaveReadMessage/{User.Identity.GetUserId()}.json");
				if (!System.IO.File.Exists(fileName)) {
					System.IO.File.WriteAllText(fileName, "[]");
				}

				string jsonContent = System.IO.File.ReadAllText(fileName);
				return Json(
					new
					{
						Success = true,
						Data = jsonContent
					},
					JsonRequestBehavior.AllowGet
				);
			}
			catch
			{
				return Json(new { Error = true }, JsonRequestBehavior.AllowGet);
			}
		}
		#endregion

		#region 小鈴鐺已讀列表 - 增加
		[HttpPost]
		public ActionResult PostHaveReadMessage(List<string> WMSNs)
		{
			try
			{
				string folderPath = Server.MapPath("~/Files/HaveReadMessage");
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}

				string fileName = Server.MapPath($"~/Files/HaveReadMessage/{User.Identity.GetUserId()}.json");
				if (!System.IO.File.Exists(fileName)) {
					System.IO.File.WriteAllText(fileName, "[]");
				}

				string jsonContent = System.IO.File.ReadAllText(fileName);
				JArray data = (JArray)JsonConvert.DeserializeObject(jsonContent);
				HashSet<string> set = new HashSet<string>();

				foreach (string WMSN in data)
				{
					set.Add(WMSN);
				}

				foreach (string WMSN in WMSNs)
				{
					set.Add(WMSN);
				}

				JArray jsonArray = new JArray(set.Select(item => new JValue(item)));

				System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(jsonArray));
				return Json(new { Success = true });
			}
			catch
			{
				return Json(new { Error = true });
			}
		}
		#endregion
	}
}