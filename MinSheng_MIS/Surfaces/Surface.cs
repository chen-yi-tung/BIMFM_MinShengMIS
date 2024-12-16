using MinSheng_MIS.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using static MinSheng_MIS.Services.UniParams.MaintainPeriod;

namespace MinSheng_MIS.Surfaces
{
    public class Surface
    {
        //5-2-1 巡檢計畫狀態編碼對照
        #region InspectionPlanState 巡檢計畫狀態
        public static Dictionary<string, string> InspectionPlanState() {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待執行");
            ValueOption.Add("2", "巡檢中");
            ValueOption.Add("3", "巡檢完成");
            ValueOption.Add("4", "巡檢未完成");
            ValueOption.Add("5", "停用"); 
            return ValueOption;
        }
        #endregion

        //5-2-2 保養相關狀態編碼對照
        #region InspectionPlanMaintainState 巡檢計畫中保養計畫狀態
        public static Dictionary<string, string> InspectionPlanMaintainState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "已派工");
            ValueOption.Add("2", "施工中");
            ValueOption.Add("3", "待審核");
            ValueOption.Add("4", "未完成");
            ValueOption.Add("5", "待補件");
            ValueOption.Add("6", "完成");
            ValueOption.Add("7", "審核未過");
            return ValueOption;
        }
        #endregion

        #region EquipmentMaintainFormItemState 設備保養單各項目狀態
        public static Dictionary<string, string> EquipmentMaintainFormItemState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待派工");
            ValueOption.Add("2", "已派工");
            ValueOption.Add("3", "施工中");
            ValueOption.Add("4", "待審核");
            ValueOption.Add("5", "未完成");
            ValueOption.Add("6", "待補件");
            ValueOption.Add("7", "完成");
            ValueOption.Add("8", "審核未過");
            ValueOption.Add("9", "保留中(待派工)");
            ValueOption.Add("10", "保留中(未完成)");
            ValueOption.Add("11", "保留中(審核未過)");
            return ValueOption;
        }
        #endregion

        #region EquipmentMaintainItemState 設備保養項目狀態
        public static Dictionary<string, string> EquipmentMaintainItemState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待產單");
            ValueOption.Add("2", "已產單");
            return ValueOption;
        }
        #endregion

        #region MaintainItemIsEnable 保養項目是否被刪除
        public static Dictionary<string, string> MaintainItemIsEnable()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("0", "停用");
            ValueOption.Add("1", "啟用");
            return ValueOption;
        }
        #endregion

        #region MaintainPeriod 保養週期
        public static Dictionary<string, string> MaintainPeriod()
        {
            return Enum.GetValues(typeof(Period))
                     .Cast<Period>()
                     .ToDictionary(
                         period => period.ToString(),
                         period => period.GetLabel()
                     );

            //return new Dictionary<string, string>
            //{
            //    { Period.Daily.ToString(), Period.Daily.GetLabel() },
            //    { Period.Monthly.ToString(), Period.Monthly.GetLabel() },
            //    { Period.Quarterly.ToString(), Period.Quarterly.GetLabel() },
            //    { Period.Yearly.ToString(), Period.Yearly.GetLabel() }
            //};
        }
        #endregion

        //5-2-3 維修相關狀態編碼對照
        #region InspectionPlanRepairState 巡檢計畫中維修單狀態
        public static Dictionary<string, string> InspectionPlanRepairState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "已派工");
            ValueOption.Add("2", "施工中");
            ValueOption.Add("3", "待審核");
            ValueOption.Add("4", "未完成");
            ValueOption.Add("5", "待補件");
            ValueOption.Add("6", "完成");
            ValueOption.Add("7", "審核未過");
            return ValueOption;
        }
        #endregion

        #region EquipmentReportFormState 設備報修單狀態
        public static Dictionary<string, string> EquipmentReportFormState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待派工");
            ValueOption.Add("2", "已派工");
            ValueOption.Add("3", "施工中");
            ValueOption.Add("4", "待審核");
            ValueOption.Add("5", "未完成");
            ValueOption.Add("6", "待補件");
            ValueOption.Add("7", "完成");
            ValueOption.Add("8", "審核未過");
            ValueOption.Add("9", "保留中(待派工)");
            ValueOption.Add("10", "保留中(未完成)");
            ValueOption.Add("11", "保留中(審核未過)");
            return ValueOption;
        }
        #endregion

        //5-2-4 設備狀態編碼對照
        #region EState 設備狀態
        public static Dictionary<string, string> EState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "正常");
            ValueOption.Add("2", "報修中");
            ValueOption.Add("3", "停用");
            return ValueOption;
        }
        #endregion

        //5-2-5 審核結果編碼對照
        #region AuditResult 審核結果
        public static Dictionary<string, string> AuditResult()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "審核通過");
            ValueOption.Add("2", "審核未過");
            ValueOption.Add("3", "請補件");
            return ValueOption;
        }
        #endregion

        //5-2-6 報修等級編碼對照
        #region ReportLevel 報修等級
        //public static Dictionary<string, string> ReportLevel()
        //{
        //    var ValueOption = new Dictionary<string, string>();
        //    ValueOption.Add("1", "一般");
        //    ValueOption.Add("2", "緊急");
        //    ValueOption.Add("3", "最速件");
        //    return ValueOption;
        //}
        #endregion

        //5-2-7 班別對照表
        #region Shift 班別
        public static Dictionary<string, string> Shift()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "早班");
            ValueOption.Add("2", "午班");
            ValueOption.Add("3", "晚班");
            return ValueOption;
        }
        #endregion

        //5-2-8 報修來源對照表
        #region ReportSource 報修來源
        public static Dictionary<string, string> ReportSource()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "APP");
            ValueOption.Add("2", "BIM模型");
            ValueOption.Add("3", "以資查圖");
            ValueOption.Add("4", "一般報修");
            return ValueOption;
        }
        #endregion

        //5-2-9 帳號管理-權限對照表
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

        //5-2-10 請購單狀態編碼
        #region PRState 請購單狀態編碼
        public static Dictionary<string, string> PRState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待審核");
            ValueOption.Add("2", "已送審");
            ValueOption.Add("3", "審核完成");
            ValueOption.Add("4", "審核未過");
            return ValueOption;
        }
        #endregion

        //5-2-11 請購單項目狀態
        #region PRIState 請購單項目狀態
        public static Dictionary<string, string> PRIState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待審核");
            ValueOption.Add("2", "審核通過");
            ValueOption.Add("3", "審核未過");
            ValueOption.Add("4", "待採購");
            ValueOption.Add("5", "已下單");
            return ValueOption;
        }
        #endregion

        //5-2-12 詢價單狀態
        #region IOState 詢價單狀態
        public static Dictionary<string, string> IOState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "已建立");
            ValueOption.Add("2", "待審核");
            ValueOption.Add("3", "審核通過");
            ValueOption.Add("4", "審核未過");
            ValueOption.Add("5", "衝突");
            return ValueOption;
        }
        #endregion

        //5-2-13 採購單狀態
        #region POState 採購單狀態
        public static Dictionary<string, string> POState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待採購");
            ValueOption.Add("2", "已下單");
            ValueOption.Add("3", "部分驗收");
            ValueOption.Add("4", "驗收完成");
            ValueOption.Add("5", "撤銷");
            return ValueOption;
        }
        #endregion

        //5-2-14 採購單項目狀態
        #region State 採購單項目狀態
        public static Dictionary<string, string> State()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待採購");
            ValueOption.Add("2", "已下單");
            ValueOption.Add("3", "待驗收");
            ValueOption.Add("4", "合格");
            ValueOption.Add("5", "不合格");
            ValueOption.Add("6", "已入庫");
            ValueOption.Add("7", "退貨折讓");
            return ValueOption;
        }
        #endregion

        //5-2-15 領用申請單狀態
        #region SRState 領用申請單狀態
        public static Dictionary<string, string> SRState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待審核");
            ValueOption.Add("2", "審核完成");
            return ValueOption;
        }
        #endregion

        //5-2-16 領用申請單項目狀態
        #region PickUpStatus 領用申請單項目狀態
        public static Dictionary<string, string> PickUpStatus()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "待審核");
            ValueOption.Add("2", "審核未過");
            ValueOption.Add("3", "尚未領取");
            ValueOption.Add("4", "部分領取");
            ValueOption.Add("5", "已領取");
            return ValueOption;
        }
        #endregion

        //5-2-17 庫存品項編碼
        #region StockType 庫存品項編碼
        public static Dictionary<string, string> StockType()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("A", "設備零件");
            ValueOption.Add("B", "備料");
            ValueOption.Add("C", "補給品");
            ValueOption.Add("D", "備品");
            ValueOption.Add("E", "藥劑");
            ValueOption.Add("F", "替代料件");
            return ValueOption;
        }
        #endregion

        //庫存狀態
        #region StockState 庫存狀態
        public static Dictionary<string, string> StockState()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("0", "無");
            ValueOption.Add("1", "有");
            return ValueOption;
        }
        #endregion

        //單位
        #region 單位 Unit
        public static Dictionary<string, string> Unit()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("01", "個");
            ValueOption.Add("02", "顆");
            ValueOption.Add("03", "瓶");
            ValueOption.Add("04", "片");
            ValueOption.Add("05", "條");
            ValueOption.Add("06", "支");
            ValueOption.Add("07", "根");
            ValueOption.Add("08", "盒");
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
            ValueOption.Add("3", "OO類");
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