using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Surfaces
{
    public class Surface
    {
        //InspectionPlanState 巡檢計畫狀態
        public static Dictionary<string, string> InspectionPlanState() {
            var aaa = new Dictionary<string, string>();
            aaa.Add("1", "待執行");
            aaa.Add("2", "巡檢中");
            aaa.Add("3", "巡檢完成");
            aaa.Add("4", "巡檢未完成");
            aaa.Add("5", "停用"); 
            return aaa;
        }

        //EquipmentReportFormState 設備報修單狀態
        public static Dictionary<string, string> EquipmentReportFormState()
        {
            var aaa = new Dictionary<string, string>();
            aaa.Add("1", "待派工");
            aaa.Add("2", "已派工");
            aaa.Add("3", "保養中");
            aaa.Add("4", "待審核");
            aaa.Add("5", "未完成");
            aaa.Add("6", "待補件");
            aaa.Add("7", "完成");
            aaa.Add("8", "審核未過");
            aaa.Add("9", "保留中(待派工)");
            aaa.Add("10", "保留中(未完成)");
            aaa.Add("11", "保留中(審核未過)");
            return aaa;
        }

    }
}