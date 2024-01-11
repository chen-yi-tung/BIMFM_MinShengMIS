using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class WarningMessageService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        public void AddWarningMessageFillinRecord(FillinInfo info,string UserName) //新增警示訊息填報紀錄
        {
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
    }
}