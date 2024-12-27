using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MinSheng_MIS.Models.ViewModels
{
    #region 工單-新增
    public class InspectionPlanCreateViewModel : ICreateInspectionPlan
    {
        [Required]
        [StringLength(20, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "工單名稱")]
        public string IPName { get; set; } // 工單名稱
        [Required]
        [Display(Name = "工單日期")]
        public DateTime PlanDate { get; set; } // 工單日期
        [Required]
        [Display(Name = "工單巡檢內容")]
        public IEnumerable<InspectionPlanContent> Inspections { get; set; } // 工單巡檢內容
    }

    public interface ICreateInspectionPlan : IInspectionPlanModifiable
    {
        IEnumerable<InspectionPlanContent> Inspections { get; set; } // 工單巡檢內容
    }

    public class InspectionPlanContent : InspectionSampleContent, IInspectionPlanContent, ISamplePathInfo
    {
        string ISamplePathInfo.PathName { get; set; }
        int ISamplePathInfo.Frequency { get; set; }
        int ISamplePathInfo.EquipmentCount { get; set; }

        [Required]
        [Display(Name = "執行人員")]
        public IEnumerable<string> Executors { get; set; } // 工單執行人員
    }

    public interface IInspectionPlanContent :
        IInspectionPlanTime, IInspectionPlanExecutors
    { }
    #endregion

    #region 工單檢視
    public class InspectionPlanDetailViewModel
    {
        public string IPSN { get; set; } // 工單編號
        public string PlanState { get; set; } // 工單狀態
        public string IPName { get; set; } // 工單名稱
        public DateTime PlanDate { get; set; } // 工單日期
        public List<InspectionPlanContentDetail> Inspections { get; set; } = new List<InspectionPlanContentDetail>(); // 工單巡檢內容
    }

    /// <summary>
    /// 巡檢紀錄
    /// </summary>
    public class InspectionPlanContentDetail : IInspectionPlanContentDetail
    {
        int ISamplePathInfo.Frequency { get; set; }
        int ISamplePathInfo.EquipmentCount { get; set; }
        [JsonIgnore]
        public string IPTSN { get; set; }
        public string PathName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string InspectionState { get; set; }
        public string Frequency { get; set; }
        public string EquipmentCount { get; set; }
        public string PlanPathSN { get; set; }
        public List<IInspectionPlanEquipment> Equipments { get; set; } = new List<IInspectionPlanEquipment>();
    }

    /// <summary>
    /// 巡檢紀錄
    /// </summary>
    public interface IInspectionPlanContentDetail : IInspectionPlanTime
    {
        string IPTSN { get; set; } // 巡檢計畫時段編號
        string InspectionState { get; set; } // 巡檢狀態
        List<IInspectionPlanEquipment> Equipments { get; set; } // 巡檢設備列表
    }

    /// <summary>
    /// 巡檢路線中的設備
    /// </summary>
    public class InspectionPlanEquipment : IInspectionPlanEquipment
    {
        [JsonIgnore]
        public string IPESN { get; set; } // 巡檢設備編號
        public string EName { get; set; } // 設備名稱
        public string NO { get; set; } // 設備編號
        public string Location { get; set; } // 所在位置
        public string ReportUserName { get; set; } // 最新填報者
        public string FillinTime { get; set; } // 最新填報時間
        public IEnumerable<IInspectionPlanCheckItem> CheckItems { get; set; } // 檢查項目列表
        public IEnumerable<IInspectionPlanRportItem> RportItems { get; set; } // 填報項目列表
    }

    public interface IInspectionPlanEquipment : IEquipmentName
    {
        string IPESN { get; set; } // 巡檢設備編號
        string Location { get; set; } // 所在位置
        string ReportUserName { get; set; } // 最新填報者
        string FillinTime { get; set; } // 最新填報時間
        IEnumerable<IInspectionPlanCheckItem> CheckItems { get; set; } // 檢查項目列表
        IEnumerable<IInspectionPlanRportItem> RportItems { get; set; } // 填報項目列表
    }

    /// <summary>
    /// 設備中的檢查項目
    /// </summary>
    public class InspectionPlanCheckItem : IInspectionPlanCheckItem
    {
        public string Item { get; set; } // 檢查項目名稱
        public string Result { get; set; } // 檢查結果
    }

    public interface IInspectionPlanCheckItem
    {
        string Item { get; set; } // 檢查項目名稱
        string Result { get; set; } // 檢查結果
    }

    /// <summary>
    /// 設備中的填報項目
    /// </summary>
    public class InspectionPlanRportItem : IInspectionPlanRportItem
    {
        public string Item { get; set; } // 檢查項目名稱
        public string Value { get; set; } // 填報內容
        public string Unit { get; set; } // 填報項目單位
    }

    public interface IInspectionPlanRportItem
    {
        string Item { get; set; } // 填報項目名稱
        string Value { get; set; } // 填報內容
        string Unit { get; set; } // 填報項目單位
    }
    #endregion

    public interface IInspectionPlanModifiable
    {
        string IPName { get; set; } // 工單名稱
        DateTime PlanDate { get; set; } // 工單日期
    }

    public interface IInspectionPlanTime : ISamplePathInfo, IInspectionSampleContent { }

    public interface IInspectionPlanExecutors
    {
        IEnumerable<string> Executors { get; set; } // 工單執行人員
    }

    public interface IInspectionPlanTimeModifiableList
    {
        string IPSN { get; set; } // 巡檢計畫編號
        IEnumerable<IInspectionPlanContent> Inspections { get; set; } // 工單巡檢內容
    }

    public interface IInspectionPlanMemberModifiableList : IInspectionPlanExecutors
    {
        string IPTSN { get; set; } // 巡檢計畫時段編號
    }

    #region Service使用
    public class InspectionPlanTimeModifiableListInstance : IInspectionPlanTimeModifiableList
    {
        public string IPSN { get; set; } // 巡檢計畫編號
        public IEnumerable<IInspectionPlanContent> Inspections { get; set; } // 工單巡檢內容

        public InspectionPlanTimeModifiableListInstance(string sn, InspectionPlanCreateViewModel data)
        {
            IPSN = sn;
            Inspections = data.Inspections;
        }
    }

    public class InspectionPlanMemberModifiableListInstance : IInspectionPlanMemberModifiableList
    {
        public string IPTSN { get; set; } // 巡檢計畫時段編號
        public IEnumerable<string> Executors { get; set; } // 工單執行人員

        public InspectionPlanMemberModifiableListInstance(string sn, IInspectionPlanContent data)
        {
            IPTSN = sn;
            Executors = data.Executors;
        }
    }
    #endregion
}