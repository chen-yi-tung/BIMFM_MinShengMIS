using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public static Dictionary<string, string> ReportLevel()
        {
            var ValueOption = new Dictionary<string, string>();
            ValueOption.Add("1", "一般");
            ValueOption.Add("2", "緊急");
            ValueOption.Add("3", "最速件");
            return ValueOption;
        }
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
            return ValueOption;
        }
        #endregion
    }
}