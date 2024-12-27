using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
    #endregion

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
        IInspectionPlanTime, IInspectionPlanExecutors { }

    public interface IInspectionPlanTime : ISamplePathInfo, IInspectionSampleContent { }

    public interface IInspectionPlanExecutors
    {
        IEnumerable<string> Executors { get; set; } // 工單執行人員
    }

    public interface IInspectionPlanModifiable
    {
        string IPName { get; set; } // 工單名稱
        DateTime PlanDate { get; set; } // 工單日期
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