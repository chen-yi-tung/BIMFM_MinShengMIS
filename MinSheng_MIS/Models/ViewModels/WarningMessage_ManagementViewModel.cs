using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class WarningMessageViewModel
    {
        public string WMSN { get; set; } //警示訊息編號
        public string WMType { get; set; } //事件等級
        public string WMState { get; set; } //事件處理狀況
        public string TimeOfOccurrence { get; set; } //發生時間
        public string Location { get; set; } //發生地點
        public string Message { get; set; } //事件內容
        public List<WarningMessageFillinRecordViewModel> records { get; set; } //填報紀錄
    }
    public class WarningMessageFillinRecordViewModel
    {
        public string FillinDateTime { get; set; } //填報時間
        public string MyName { get; set; } //填報人員
        public string FillinState { get; set; } //事件處理狀況
        public string Memo { get; set; } //備註
    }
}