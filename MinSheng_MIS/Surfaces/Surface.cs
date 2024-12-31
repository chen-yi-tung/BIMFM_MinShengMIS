using MinSheng_MIS.Attributes;
using MinSheng_MIS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using static MinSheng_MIS.Services.UniParams;

namespace MinSheng_MIS.Surfaces
{
    public class Surface
    {
        //巡檢計畫狀態編碼對照
        #region InspectionPlanState 巡檢計畫狀態
        public static Dictionary<string, string> InspectionPlanState() {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待執行");
            ValueOption.Add("2", "執行中");
            ValueOption.Add("3", "完成"); 
            return ValueOption;
        }
        #endregion

        #region Status 巡檢RFID順序狀態
        public static Dictionary<string, string> Status()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待執行");
            ValueOption.Add("2", "完成");
            return ValueOption;
        }
        #endregion

        #region InspectionState 巡檢時段狀態
        public static Dictionary<string, string> InspectionState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待執行");
            ValueOption.Add("2", "執行中");
            ValueOption.Add("3", "完成");
            return ValueOption;
        }
        #endregion

        #region InspectionPlanState 巡檢頻率
        public static Dictionary<string, string> InspectionPlanFrequency()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "每1小時");
            ValueOption.Add("2", "每2小時");
            ValueOption.Add("3", "每3小時");
            ValueOption.Add("4", "每4小時");
            ValueOption.Add("6", "每6小時");
            ValueOption.Add("8", "每8小時");
            ValueOption.Add("12", "每12小時");
            ValueOption.Add("24", "每24小時");
            return ValueOption;
        }
        #endregion

        //保養相關狀態編碼對照
        #region EquipmentMaintainItemState 設備保養項目狀態
        public static Dictionary<string, string> EquipmentMaintainItemState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待產單");
            ValueOption.Add("2", "已產單");
            return ValueOption;
        }
        #endregion

        #region MaintainPeriod 保養週期
        public static Dictionary<string, string> MaintainPeriod()
        {
            return Enum.GetValues(typeof(MaintainPeriod))
                     .Cast<MaintainPeriod>()
                     .ToDictionary(
                         period => Convert.ToInt32(period).ToString(),
                         period => period.GetLabel()
                     );
        }
        #endregion

        #region 保養單狀態 MaintainStatus
        public static Dictionary<string, string> MaintainStatus()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待派工");
            ValueOption.Add("2", "待執行");
            ValueOption.Add("3", "待審核");
            ValueOption.Add("4", "審核通過");
            ValueOption.Add("5", "審核未過");
            return ValueOption;
        }
        #endregion

        //設備狀態編碼對照
        #region EState 設備狀態
        public static Dictionary<string, string> EState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "正常");
            ValueOption.Add("2", "報修中");
            return ValueOption;
        }
        #endregion

        //報修來源對照表
        #region ReportSource 報修來源
        public static Dictionary<string, string> ReportSource()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "後台");
            ValueOption.Add("2", "APP");
            return ValueOption;
        }
        #endregion

        //帳號管理-權限對照表
        #region Authority 權限
        public static Dictionary<string,string> Authority() 
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "管理人員");
            ValueOption.Add("2", "操作人員");
            ValueOption.Add("3", "一般人員");
            ValueOption.Add("4", "巡檢人員");
            return ValueOption;
        }
        #endregion

        //庫存品項狀態
        #region StockStatus 庫存品項狀態
        public static Dictionary<string, string> StockStatus()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "充足");
            ValueOption.Add("2", "低於警戒值");
            return ValueOption;
        }
        #endregion

        //設計圖說種類
        #region 設計圖說種類 ImgType
        public static Dictionary<string, string> ImgType()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "機電類");
            ValueOption.Add("2", "結構類");
            ValueOption.Add("3", "非主結構類");
            ValueOption.Add("4", "其它");
            return ValueOption;
        }
        #endregion

        //警示訊息-事件等級
        #region 事件等級 WMType
        public static Dictionary<string, string> WMType()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "一般");
            ValueOption.Add("2", "緊急");
            return ValueOption;
        }
        #endregion

        //警示訊息-事件處理狀況
        #region 事件處理狀況 WMState
        public static Dictionary<string, string> WMState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待處理");
            ValueOption.Add("2", "處理中");
            ValueOption.Add("3", "處理完成");
            return ValueOption;
        }
        #endregion

        //報修管理-報修等級
        #region 報修單狀態 ReportState
        public static Dictionary<string, string> ReportState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待派工");
            ValueOption.Add("2", "待執行");
            ValueOption.Add("3", "待審核");
            ValueOption.Add("4", "審核通過");
            ValueOption.Add("5", "審核未過");
            return ValueOption;
        }
        #endregion

        #region 報修等級 ReportLevel
        public static Dictionary<string, string> ReportLevel()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "一般");
            ValueOption.Add("2", "緊急");
            ValueOption.Add("3", "最速件");
            return ValueOption;
        }
        #endregion
    }
}