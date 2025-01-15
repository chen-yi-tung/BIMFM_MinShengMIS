using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class WarningMessageService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        #region 新增警示訊息填報紀錄
        public void AddWarningMessageFillinRecord(FillinInfo info,string UserName) //新增警示訊息填報紀錄
        {
            //依填報事件處理狀況 更新警示訊息事件處理狀況
            var message = db.WarningMessage.Find(info.WMSN);
            if(message != null)
            {
                message.WMState = info.FillinState;
                db.WarningMessage.AddOrUpdate(message);
                db.SaveChanges();
            }
            var record = new WarningMessageFillinRecord();

            //編WNFRSN
            var num = 1;
            var count = db.WarningMessageFillinRecord.Where(x => x.WMSN == info.WMSN).Count();
            if(count > 0)
            {
                num = count + 1;
            }

            record.WNFRSN = info.WMSN + "_" + num.ToString().PadLeft(2, '0');
            record.WMSN = info.WMSN;
            record.FillinDateTime = DateTime.Now;
            record.FillinUserName = UserName;
            record.FillinState = info.FillinState;
            record.Memo = info.Memo;

            db.WarningMessageFillinRecord.AddOrUpdate(record);
            db.SaveChanges();
        }
        #endregion
        #region 新增警示訊息
        public void AddWarningMessage(WarningMessageCreateModel info,string userName) //新增警示訊息
        {
            //編WMSN
            var newWMSN = "";
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var WMSN = db.WarningMessage.Where(x => x.TimeOfOccurrence >= today && x.TimeOfOccurrence < tomorrow).OrderByDescending(x => x.WMSN).FirstOrDefault()?.WMSN.ToString();
            if (WMSN != null)
            {
                newWMSN = (int.Parse(WMSN) + 1).ToString();
            }
            else
            {
                newWMSN = DateTime.Today.ToString("yyMMdd") + "0001";
            }

            var data = new WarningMessage();
            data.WMSN = newWMSN;
            data.WMType = info.WMType;
            data.WMClass = info.WMClass;
            data.WMState = "1";
            data.TimeOfOccurrence = DateTime.Now;
            data.FSN = info.FSN;
            data.Message = info.Message;
            data.UserName = userName;

            db.WarningMessage.AddOrUpdate(data);
            db.SaveChanges();
        }
        #endregion
    }
}