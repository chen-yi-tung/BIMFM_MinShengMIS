using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinSheng_MIS.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data.Entity.Migrations;
using MinSheng_MIS.Services;
using MinSheng_MIS.Models.ViewModels;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Net;
using System.Xml.Linq;

namespace MinSheng_MIS.Controllers
{
	public class MeetingMinutes_ManagementController : Controller
	{
		// GET: MeetingMinutes_Management
		Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
		static readonly string folderPath = "Files/MeetingMinutes";

		#region 會議記錄管理
		public ActionResult Index()
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
		public ActionResult CreateMeetingMinutes(MeetingMinutesInfo Info)
		{
			if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

			//MMSN
			var mmsnnum = 1;
			var currentmmsnnum = db.MeetingMinutes.Where(x => x.MeetingDate == Info.MeetingDate).Count();
			if (currentmmsnnum > 0)
			{
				mmsnnum = currentmmsnnum + 1;
			}
			Info.MMSN = Info.MeetingDate.Date.ToString("yyMMdd") + mmsnnum.ToString().PadLeft(2, '0');
			var FileName = "";
			//新增會議紀錄文件
			if (Info.MeetingFile != null)
			{
				//檢查會議記錄文件格式
				string extension = Path.GetExtension(Info.MeetingFile.FileName); //檔案副檔名
				if (ComFunc.IsConformedForDocument(Info.MeetingFile.ContentType, extension) || ComFunc.IsConformedForImage(Info.MeetingFile.ContentType, extension)) //檔案白名單檢查
				{
					#region 新增會議記錄文件
					// 檔案上傳
					if (!ComFunc.UploadFile(Info.MeetingFile, Server.MapPath($"~/{folderPath}/"), Info.MMSN))
						return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "檔案上傳過程出錯!");
					FileName = Info.MMSN + extension;
					#endregion
				}
				else
					return Content("<br>非系統可接受的檔案格式!<br>僅支援上傳圖片、Word或PDF!", "application/json; charset=utf-8");
			}
			//新增會議紀錄
			MeetingMinutesService mm = new MeetingMinutesService();
			mm.AddMeetingMinutes(Info, FileName, User.Identity.Name);
			return Content(JsonConvert.SerializeObject(new JObject { { "Succeed", true } }), "application/json");
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
				{ "MeetingContent", item.MeetingContent },
				{ "FilePath" , !string.IsNullOrEmpty(item.MeetingFile) ? "/" + folderPath + "/" + item.MeetingFile : null}
			};

			string result = JsonConvert.SerializeObject(jo);
			return Content(result, "application/json");
		}
		[HttpPost]
		public ActionResult EditMeetingMinutes(MeetingMinutesInfo Info)
		{
			if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過
			var FName = db.MeetingMinutes.Find(Info.MMSN).MeetingFile;
			var FileName = "";
			//檢查是否要編輯或刪除會議紀錄文件
			if (Info.MeetingFileName != null) //維持原檔案
			{
				FileName = FName;

			}
			else if (Info.MeetingFile != null) //新增或更新檔案
			{
				//若原有檔案則刪除舊檔案
				if (!string.IsNullOrEmpty(FName))
				{
					ComFunc.DeleteFile(Server.MapPath($"~/{folderPath}/"), FName, null);
				}
				//檢查新上傳會議記錄文件格式
				string extension = Path.GetExtension(Info.MeetingFile.FileName); //檔案副檔名
				if (ComFunc.IsConformedForDocument(Info.MeetingFile.ContentType, extension) || ComFunc.IsConformedForImage(Info.MeetingFile.ContentType, extension)) //檔案白名單檢查
				{
					#region 新增會議紀錄文件
					// 檔案上傳
					if (!ComFunc.UploadFile(Info.MeetingFile, Server.MapPath($"~/{folderPath}/"), Info.MMSN))
						return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "檔案上傳過程出錯!");
					FileName = Info.MMSN + extension;
					#endregion
				}
				else
					return Content("<br>非系統可接受的檔案格式!<br>僅支援上傳圖片、Word或PDF!", "application/json; charset=utf-8");
			}
			else //若為空且原有檔案，則刪除原有檔案
			{
				if (!string.IsNullOrEmpty(FName))
				{
					ComFunc.DeleteFile(Server.MapPath($"~/{folderPath}/"), FName, null);
				}
			}
			//編輯會議紀錄
			MeetingMinutesService mm = new MeetingMinutesService();
			mm.EditMeetingMinutes(Info, FileName, User.Identity.Name);
			return Content(JsonConvert.SerializeObject(new JObject { { "Succeed", true } }), "application/json");
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
				{ "MeetingContent", item.MeetingContent },
				{ "FilePath" , !string.IsNullOrEmpty(item.MeetingFile) ? "/" + folderPath + "/" + item.MeetingFile : null}
			};

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
		[HttpDelete]
		public ActionResult DeleteMeetingMinutes(string MMSN)
		{
			var mm = db.MeetingMinutes.Find(MMSN);
			if (mm != null)
			{
				//若原有會議紀錄檔案要刪除
                if (!string.IsNullOrEmpty(mm.MeetingFile))
				{
                    ComFunc.DeleteFile(Server.MapPath($"~/{folderPath}/"), mm.MeetingFile, null);
                }
                db.MeetingMinutes.Remove(mm);
				db.SaveChanges();
				return Content(JsonConvert.SerializeObject(new JObject { { "Succeed", true } }), "application/json");
			}
            return new HttpNotFoundResult("查無資料，無法刪除");
        }
		#endregion
	}
}