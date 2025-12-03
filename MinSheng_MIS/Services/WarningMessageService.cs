using MinSheng_MIS.Attributes;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using static MinSheng_MIS.Services.UniParams;

namespace MinSheng_MIS.Services
{
    public class WarningMessageService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;

        private readonly Dictionary<WMClass, string> _msgContentMapping = new Dictionary<WMClass, string>
        {
            { WMClass.AbnormalHeartRate, "{0} 心率異常 ({1}下/分)" },
            { WMClass.RouteDeviation, "巡檢路線偏移" },
            { WMClass.ProlongedStop, "{0} 停留過久 ({1})" },
            { WMClass.EmergencySituation, "{0} 觸發緊急按鈕" },
        };

        private readonly Dictionary<WMClass, WMType> _waringTypeMapping = new Dictionary<WMClass, WMType>
        {
            { WMClass.AbnormalHeartRate, WMType.Emergency },
            { WMClass.RouteDeviation, WMType.General },
            { WMClass.ProlongedStop, WMType.Emergency },
            { WMClass.EmergencySituation, WMType.Emergency },
        };

        public WarningMessageService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        #region 新增警示訊息填報紀錄
        public void AddWarningMessageFillinRecord(FillinInfo info,string UserName) //新增警示訊息填報紀錄
        {
            //依填報事件處理狀況 更新警示訊息事件處理狀況
            var message = _db.WarningMessage.Find(info.WMSN);
            if(message != null)
            {
                message.WMState = info.FillinState;
                _db.WarningMessage.AddOrUpdate(message);
                _db.SaveChanges();
            }
            var record = new WarningMessageFillinRecord();

            //編WNFRSN
            var num = 1;
            var count = _db.WarningMessageFillinRecord.Where(x => x.WMSN == info.WMSN).Count();
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

            _db.WarningMessageFillinRecord.AddOrUpdate(record);
            _db.SaveChanges();
        }
        #endregion

        #region 新增警示訊息
        public async Task AddWarningMessageAsync(ICreateWarningMessage info, string otherInfo = null)
        {
            if (info.WMClass == WMClass.AbnormalHeartRate || info.WMClass == WMClass.ProlongedStop)
            {
                var userLastWarning = await _db.WarningMessage
                .Where(x => x.UserName == info.UserName && x.WMClass == ((int)info.WMClass).ToString())
                .OrderByDescending(x => x.TimeOfOccurrence)
                .FirstOrDefaultAsync();

                if (userLastWarning != null && userLastWarning.WMState != ((int)WMState.Completed).ToString())
                    // 已有未處理的相同使用者心率異常/停留過久警示訊息，跳過新增
                    return;
            }

            var latest = await _db.WarningMessage.OrderByDescending(x => x.WMSN).FirstOrDefaultAsync();

            var warningMsg = new WarningMessage
            {
                WMSN = ComFunc.GenerateUniqueSn("!{yyMMdd}%{4}", 10, latest?.WMSN),
                WMType = _waringTypeMapping.TryGetValue(info.WMClass, out var type) ? 
                    ((int)type).ToString() : 
                    ((int)WMType.General).ToString(),
                WMClass = ((int)info.WMClass).ToString(),
                WMState = ((int)WMState.Pending).ToString(),
                TimeOfOccurrence = DateTime.Now,
                FSN = info.FSN,
                Message = _msgContentMapping.TryGetValue(info.WMClass, out var msg) ?
                    string.Format(msg, info.UserName, otherInfo) :
                    $"未知告警! ({info.WMClass.GetLabel()})",
                UserName = info.UserName,
                Location_X = info.Location_X,
                Location_Y = info.Location_Y
            };

            _db.WarningMessage.AddOrUpdate(warningMsg);
            await _db.SaveChangesAsync();
        }
        #endregion
    }
}