using MinSheng_MIS.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MinSheng_MIS.Models.ViewModels
{
    #region 每日巡檢時程安排-新增
    public class SampleScheduleCreateViewModel : ICreateSampleSchedule
    {
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "巡檢模板名稱")]
        public string TemplateName { get; set; } // 巡檢模板名稱
        [Required]
        [Display(Name = "每日模板內容")]
        public IEnumerable<InspectionSampleContent> Contents { get; set; } // 每日模板內容
    }
    #endregion

    #region 每日巡檢時程安排-詳情
    public class SampleScheduleDetailViewModel
    {
        public string TemplateName { get; set; } // 巡檢模板名稱
        public IEnumerable<ISampleScheduleContentDetail> Contents { get; set; } // 每日模板內容
    }

    // <summary>
    /// 每日模板內容
    /// </summary>
    public class SampleScheduleContentDetailModel : ISampleScheduleContentDetail, ISamplePathInfo
    {
        int ISamplePathInfo.Frequency { get; set; }
        int ISamplePathInfo.EquipmentCount { get; set; }

        public string StartTime { get; set; } // 巡檢時間(起)
        public string EndTime { get; set; } // 巡檢時間(迄)
        public string PathName { get; set; } // 巡檢路線名稱
        public string Frequency { get; set; } // 巡檢頻率
        public string EquipmentCount { get; set; } // 巡檢數量
        public string PlanPathSN { get; set; } // 巡檢路線編號
    }
    #endregion

    public class InspectionSampleContent : IInspectionSampleContent
    {
        [Required]
        [StringLength(5, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [ContinentalTime]
        [Display(Name = "巡檢時間(起)")]
        public string StartTime { get; set; } // 巡檢時間(起)
        [Required]
        [StringLength(5, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [ContinentalTime]
        [Display(Name = "巡檢時間(迄)")]
        public string EndTime { get; set; } // 巡檢時間(迄)
        [Required]
        [StringLength(9, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "巡檢路線編號")]
        public string PlanPathSN { get; set; } // 巡檢路線編號
    }

    public interface IInspectionSampleContent
    {
        string StartTime { get; set; } // 巡檢時間(起)
        string EndTime { get; set; } // 巡檢時間(迄)
        string PlanPathSN { get; set; } // 巡檢路線編號
    }

    public interface ICreateSampleSchedule : IInspectionSampleInfoModifiable
    {
        IEnumerable<InspectionSampleContent> Contents { get; set; } // 每日模板內容
    }

    public interface ISampleScheduleContentDetail : ISamplePathInfo, IInspectionSampleContent
    {
        new string Frequency { get; set; } // 巡檢頻率
        new string EquipmentCount { get; set; } // 巡檢數量
    }

    public interface ISamplePathInfo
    {
        string PathName { get; set; } // 巡檢路線名稱
        int Frequency { get; set; } // 巡檢頻率
        int EquipmentCount { get; set; } // 巡檢數量
    }

    public interface IInspectionSampleInfoModifiable
    {
        string TemplateName { get; set; } // 巡檢模板名稱
    }

    public interface IInspectionSampleContentModifiableList
    {
        string DailyTemplateSN { get; set; } // 每日模板編號
        IEnumerable<InspectionSampleContent> Contents { get; set; } // 每日模板內容
    }

    #region Service使用
    public class SampleContentModifiableListInstance : IInspectionSampleContentModifiableList
    {
        public string DailyTemplateSN { get; set; } // 每日模板編號
        public IEnumerable<InspectionSampleContent> Contents { get; set; } // 每日模板內容

        public SampleContentModifiableListInstance() { }

        public SampleContentModifiableListInstance(string sn, SampleScheduleCreateViewModel data)
        {
            DailyTemplateSN = sn;
            Contents = data.Contents;
        }
    }
    #endregion
}