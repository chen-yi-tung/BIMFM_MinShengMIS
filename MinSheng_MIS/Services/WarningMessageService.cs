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
        public async Task AddOrUpdateWarningMessageAsync(ICreateWarningMessage info, string otherInfo = null)
        {
            // 預先處理常數，避免在 LINQ 中重複轉型
            string wmClassStr = ((int)info.WMClass).ToString();
            string stateCompleted = ((int)WMState.Completed).ToString();
            DateTime today = DateTime.Now.Date;

            switch (info.WMClass)
            {
                case WMClass.AbnormalHeartRate:
                    //  取得當前使用者今日未有地點的未復歸心率異常警示訊息
                    var candidates = await _db.WarningMessage
                        .Where(x => x.TimeOfOccurrence >= today
                                    && x.UserName == info.UserName
                                    && x.WMClass == wmClassStr
                                    && x.WMState != stateCompleted
                                    && (x.FSN == null || x.FSN == info.FSN)) // 條件合併查詢
                        .ToListAsync();

                    var uncompletedNoLocationWarning = candidates.FirstOrDefault(x => x.FSN == null);

                    // 當前無地點
                    if (info.FSN == null)
                        if (uncompletedNoLocationWarning == null)
                            break; // 沒有無地點的警示紀錄 => 跳出，新增一筆
                        else
                            return; // 有無地點的警示紀錄 => 回傳，不新增
                    // 當前有地點
                    else
                    {
                        // 取得當前使用者今日同當前地點的未復歸心率異常警示訊息
                        var uncompletedSameLocationWarning = candidates.FirstOrDefault(x => x.FSN == info.FSN);
                        // 有無地點警示紀錄，且有同當前地點警示紀錄
                        if (uncompletedNoLocationWarning != null && uncompletedSameLocationWarning != null)
                        {
                            // 刪除無地點的警示紀錄
                            _db.WarningMessage.Remove(uncompletedNoLocationWarning);
                            await _db.SaveChangesAsync();
                            return;
                        }
                        // 有無地點警示紀錄，且沒有同當前地點警示紀錄
                        else if (uncompletedNoLocationWarning != null && uncompletedSameLocationWarning == null)
                        {
                            // 更新無地點的警示紀錄為當前地點
                            uncompletedNoLocationWarning.FSN = info.FSN;
                            uncompletedNoLocationWarning.Location_X = info.Location_X;
                            uncompletedNoLocationWarning.Location_Y = info.Location_Y;
                            _db.WarningMessage.AddOrUpdate(uncompletedNoLocationWarning);
                            await _db.SaveChangesAsync();
                            return;
                        }
                        // 沒有無地點警示紀錄，且有同當前地點警示紀錄
                        else if (uncompletedNoLocationWarning == null && uncompletedSameLocationWarning != null)
                            return; // 有同當前地點的警示紀錄 => 回傳，不新增
                        else 
                            break; // 跳出，新增一筆
                    }
                    break;
                case WMClass.ProlongedStop:
                    var userLastWarning = await _db.WarningMessage
                        .Where(x => x.TimeOfOccurrence >= today
                            && x.UserName == info.UserName 
                            && x.WMClass == wmClassStr
                            && x.FSN == info.FSN
                            && x.WMState != stateCompleted)
                        .FirstOrDefaultAsync();
                    if (userLastWarning != null)
                        return;
                    break;

                default:
                    break;
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