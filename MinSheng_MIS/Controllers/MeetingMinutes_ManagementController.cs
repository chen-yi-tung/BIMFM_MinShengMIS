using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data.Entity.Migrations;

namespace MinSheng_MIS.Controllers
{
	public class MeetingMinutes_ManagementController : Controller
	{
		// GET: MeetingMinutes_Management
		Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
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
		[HttpPost]
		public ActionResult CreateMeetingMinutes(FormCollection form)
		{
			DateTime today = DateTime.Now.Date;
			try
			{
				#region 取得新SN
				var lastMMSN = db.MeetingMinutes.Where(x => x.UploadDateTime == today).OrderByDescending(x => x.MMSN).FirstOrDefault();
				var num = 1;
				if (lastMMSN != null)
				{
					num = Convert.ToInt32(lastMMSN.MMSN) % 100 + 1;
				}
				var newMMSN = today.ToString("yyMMdd") + num.ToString().PadLeft(2, '0');
				#endregion

                MeetingMinutes mm = new MeetingMinutes
                {
                    MMSN = newMMSN,
                    MeetingTopic = form["MeetingTopic"].ToString(),
					MeetingVenue = form["MeetingVenue"].ToString(),
					Chairperson = form["Chairperson"].ToString(),
					Participant = form["Participant"].ToString(),
					ExpectedAttendence = Convert.ToInt32(form["ExpectedAttendence"]),
					ActualAttendence = Convert.ToInt32(form["ActualAttendence"]),
					AbsenteeList = Convert.ToInt32(form["AbsenteeList"]),
					TakeTheMinutes = form["TakeTheMinutes"].ToString(),
                    UploadDateTime = DateTime.Now,
                    UploadUserName = User.Identity.Name,
                };

				if (!string.IsNullOrEmpty(form["Agenda"]?.ToString())) {
					if (form["Agenda"].ToString().Length >= 256) {
						return Content("議題順序 文字長度須小於256字", "application/json");
					}
					mm.Agenda = form["Agenda"].ToString();
				}
				if (!string.IsNullOrEmpty(form["MeetingContent"]?.ToString())) {
					mm.MeetingContent = form["MeetingContent"].ToString();
				}

				#region 處理日期 讓開會時間的年月日同步
				DateTime MeetingDate = Convert.ToDateTime(form["MeetingDate"].ToString());
				DateTime MeetingDateStart = Convert.ToDateTime(form["MeetingDateStart"].ToString());
				DateTime MeetingDateEnd = Convert.ToDateTime(form["MeetingDateEnd"].ToString());

				MeetingDateStart.AddYears(MeetingDate.Year - MeetingDateStart.Year);
				MeetingDateStart.AddMonths(MeetingDate.Month - MeetingDateStart.Month);
				MeetingDateStart.AddDays(MeetingDate.Day - MeetingDateStart.Day);

				MeetingDateEnd.AddYears(MeetingDate.Year - MeetingDateEnd.Year);
				MeetingDateEnd.AddMonths(MeetingDate.Month - MeetingDateEnd.Month);
				MeetingDateEnd.AddDays(MeetingDate.Day - MeetingDateEnd.Day);

				mm.MeetingDate = MeetingDate;
				mm.MeetingDateStart = MeetingDateStart;
				mm.MeetingDateEnd = MeetingDateEnd;
				#endregion

				#region MeetingFiles TODO
				#endregion

                db.MeetingMinutes.AddOrUpdate(mm);
				db.SaveChanges();
				return Content(JsonConvert.SerializeObject(new JObject { { "Succeed", true } }), "application/json");
			}
			catch (Exception ex)
			{
				return Content(ex.Message, "application/json");
			}
		}
		#endregion

		#region 編輯會議記錄
		public ActionResult Edit(string id)
		{
			ViewBag.id = id;
			return View();
		}
		public ActionResult EditReadBody(string id)
		{
			var item = db.MeetingMinutes.Find(id);
			if (item == null) return new HttpNotFoundResult("無此資料");
			JObject jo = new JObject
			{
				{ "MMSN", item.MMSN },
				{ "MeetingTopic", item.MeetingTopic },
				{ "MeetingDate", $"{item.MeetingDate:yyyy-MM-dd}" },
				{ "MeetingDateStart", $"{item.MeetingDateStart:hh:mm}" },
				{ "MeetingDateEnd", $"{item.MeetingDateEnd:hh:mm}" },
				{ "MeetingVenue", item.MeetingVenue },
				{ "Chairperson", item.Chairperson },
				{ "Participant", item.Participant },
				{ "ExpectedAttendence", item.ExpectedAttendence },
				{ "ActualAttendence", item.ActualAttendence },
				{ "AbsenteeList", item.AbsenteeList },
				{ "TakeTheMinutes", item.TakeTheMinutes },
				{ "Agenda", item.Agenda },
				{ "MeetingContent", item.MeetingContent }
			};
			//TODO FilePath

			string result = JsonConvert.SerializeObject(jo);
			return Content(result, "application/json");
		}
		#endregion

		#region 會議記錄詳情
		public ActionResult Read(string id)
		{
			ViewBag.id = id;
			return View();
		}
		[HttpGet]
		public ActionResult ReadBody(string id)
		{
			var item = db.MeetingMinutes.Find(id);
			if (item == null) return new HttpNotFoundResult("無此資料");
			JObject jo = new JObject
			{
				{ "MMSN", item.MMSN },
				{ "MeetingTopic", item.MeetingTopic },
				{ "MeetingDate", $"{item.MeetingDate:yyyy/MM/dd}" },
				{ "MeetingTime", $"{item.MeetingDateStart:hh:mm}-{item.MeetingDateEnd:hh:mm}" },
				{ "MeetingVenue", item.MeetingVenue },
				{ "Chairperson", item.Chairperson },
				{ "Participant", item.Participant },
				{ "ExpectedAttendence", $"應到:{item.ExpectedAttendence}位  出席:{item.ActualAttendence}位  缺席:{item.AbsenteeList}位" },
				{ "TakeTheMinutes", item.TakeTheMinutes },
				{ "Agenda", item.Agenda },
				{ "MeetingContent", item.MeetingContent }
			};
			//TODO FilePath

			string result = JsonConvert.SerializeObject(jo);
			return Content(result, "application/json");
		}
		#endregion

		#region 刪除會議記錄
		public ActionResult Delete(string id)
		{
			ViewBag.id = id;
			return View();
		}
		#endregion
	}
}