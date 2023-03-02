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

    }
}