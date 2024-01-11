using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public ActionResult Read(string id)
		{
			ViewBag.id = id;
            return View();
		}
		[HttpGet]
		public ActionResult GetWarningMessageInfo(string WMSN)
		{
			var messageinfo = db.WarningMessage.Find(WMSN);
			var WMTypedic = Surface.WMType();
			var WMState = Surface.WMState();
			var FloorName = db.Floor_Info.Find(messageinfo.FSN).FloorName.ToString();
			var Area = db.Floor_Info.Find(messageinfo.FSN).AreaInfo.Area.ToString();

            WarningMessageViewModel warningMessage = new WarningMessageViewModel();
			warningMessage.WMSN = WMSN;
			warningMessage.WMType = WMTypedic[messageinfo.WMType];
			warningMessage.WMState = WMState[messageinfo.WMState];
			warningMessage.TimeOfOccurrence = messageinfo.TimeOfOccurrence.ToString("yyyy/MM/dd HH:mm:ss");
			warningMessage.Location = Area + " " + FloorName;
			warningMessage.Message = messageinfo.Message;

			//填報紀錄
			var records = db.WarningMessageFillinRecord.Where(x => x.WMSN == WMSN).ToList();
			if (records.Count() > 0)
			{
                List<WarningMessageFillinRecordViewModel> rlist = new List<WarningMessageFillinRecordViewModel>();
                foreach (var record in records)
				{
					WarningMessageFillinRecordViewModel r = new WarningMessageFillinRecordViewModel();
					r.FillinDateTime = record.FillinDateTime.ToString("yyyy/MM/dd HH:mm:ss");
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
    }
}