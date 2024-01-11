using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
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
				foreach(var record in records)
				{
					WarningMessageFillinRecordViewModel r = new WarningMessageFillinRecordViewModel();
					r.FillinDateTime = record.FillinDateTime.ToString("yyyy/MM/dd HH:mm:ss");
					r.MyName = db.AspNetUsers.Where(x => x.UserName == record.FillinUserName).FirstOrDefault().MyName.ToString();
					r.FillinState = WMState[record.FillinState];
					r.Memo = record.Memo;
					warningMessage.records.Add(r);
                }
			}
			else
			{
				warningMessage.records = null;

            }

            string result = JsonConvert.SerializeObject(warningMessage);
            return Content(result, "application/json");
        }
        #endregion
    }
}