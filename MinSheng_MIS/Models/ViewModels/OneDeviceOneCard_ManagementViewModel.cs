using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MinSheng_MIS.Models.ViewModels
{
    #region 一機一卡-新增
    public class DeviceCardCreateModel : UpdateDeviceCard
    {
        public List<CreateAddFieldModel> AddItemList { get; set; } // 增設基本資料欄位
        public List<CreateMaintainItemModel> MaintainItemList { get; set; } // 保養項目
        public List<CreateCheckItemModel> CheckItemList { get; set; } // 檢查項目
        public List<CreateReportItemModel> ReportItemList { get; set; } // 填報項目名稱/單位

        #region Implement IUpdateAddFieldList
        private class UpdateAddFieldListInstance : IUpdateAddFieldList
        {
            public string TSN { get; set; }
            public IEnumerable<string> AFNameList { get; set; } // 增設基本資料欄位
        }

        internal IUpdateAddFieldList ConvertToUpdateAddFieldList(string tsn)
        {
            return new UpdateAddFieldListInstance
            {
                TSN = tsn,
                AFNameList = this.AddItemList.Select(x => x.Value).AsEnumerable()
            };
        }
        #endregion

        #region Implement IUpdateMaintainItemList
        private class UpdateMaintainItemListInstance : IUpdateMaintainItemList
        {
            public string TSN { get; set; }
            public IEnumerable<string> MINameList { get; set; } // 保養項目名稱列表
        }

        internal IUpdateMaintainItemList ConvertToUpdateMaintainItemList(string tsn)
        {
            return new UpdateMaintainItemListInstance
            {
                TSN = tsn,
                MINameList = this.MaintainItemList.Select(x => x.Value).AsEnumerable()
            };
        }
        #endregion

        #region Implement IUpdateCheckItemList
        private class UpdateCheckItemListInstance : IUpdateCheckItemList
        {
            public string TSN { get; set; }
            public int? Frequency { get; set; }
            public IEnumerable<string> CINameList { get; set; } // 檢查項目名稱列表
        }

        internal IUpdateCheckItemList ConvertToUpdateCheckItemList(string tsn)
        {
            return new UpdateCheckItemListInstance
            {
                TSN = tsn,
                Frequency = this.Frequency,
                CINameList = this.CheckItemList.Select(x => x.Value).AsEnumerable()
            };
        }
        #endregion

        #region Implement IUpdateReportItemList
        private class UpdateReportItemListInstance : IUpdateReportItemList
        {
            public string TSN { get; set; }
            public int? Frequency { get; set; }
            public IEnumerable<UpdateReportItemModel> RIList { get; set; } // 填報項目列表
        }

        internal IUpdateReportItemList ConvertToUpdateReportItemList(string tsn)
        {
            return new UpdateReportItemListInstance
            {
                TSN = tsn,
                Frequency = this.Frequency,
                RIList = this.ReportItemList.Select(x => new UpdateReportItemModel { RIName = x.Value, Unit = x.Unit}).AsEnumerable()
            };
        }
        #endregion
    }
    #endregion

    #region 一機一卡-詳情
    /// <summary>
    /// 增設基本資料欄位
    /// </summary>
    public class AddFieldViewModel
    {
        public string SN { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// 保養資訊(與設備的一機一卡資訊共用)
    /// </summary>
    public class MaintainItemViewModel
    {
        public string SN { get; set; }
        public string Item { get; set; }
        public string Value { get; set; }
    }
    /// <summary>
    /// 巡檢資訊(與設備的一機一卡資訊共用)
    /// </summary>
    public class InspectionViewModel
    {
        public string Frequency { get; set; }
        public List<InspectionItemViewModel> InspectionItems { get; set; }
        public List<ReportItemViewModel> ReportItems { get; set; }
    }

    public class InspectionItemViewModel
    {
        public string SN { get; set; }
        public string Value { get; set; }
    }

    public class ReportItemViewModel
    {
        public string SN { get; set; }
        public string Item { get; set; }
        public string Value { get; set; }
    }
    #endregion

    #region OneDeviceOneCard 一機一卡
    public interface IDeviceCard
    {
        string SampleName { get; set; } // 模板名稱
        int? Frequency { get; set; } // 巡檢頻率
    }

    public interface ICreateDeviceCard : IDeviceCard { }

    public interface IUpdateDeviceCard : IDeviceCard { }

    public abstract class UpdateDeviceCard : IUpdateDeviceCard
    {
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "模板名稱")]
        public virtual string SampleName { get; set; } // 模板名稱
        public int? Frequency { get; set; } // 巡檢頻率
    }
    #endregion

    #region AddField 增設基本資料欄位
    public interface IAddFieldName
    {
        string Value { get; set; } // 欄位名稱
    }

    public class CreateAddFieldModel : IAddFieldName
    {
        [Required]
        [StringLength(20, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "增設基本資料欄位名稱")]
        public string Value { get; set; } // 欄位名稱
    }

    //public interface ICreateAddFieldList
    //{
    //    List<CreateAddFieldModel> AddItemList { get; set; } // 增設基本資料欄位
    //}

    public interface IUpdateAddFieldList
    {
        string TSN { get; set; } // 一機一卡模板編號
        IEnumerable<string> AFNameList { get; set ; } // 增設基本資料欄位名稱列表
    }
    #endregion

    #region MaintainItem 保養項目
    public interface IMaintainItemName
    {
        string Value { get; set; } // 保養項目名稱
    }

    public class CreateMaintainItemModel : IMaintainItemName
    {
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "保養項目名稱")]
        public string Value { get; set; } // 保養項目名稱
    }

    //public interface ICreateMaintainItemList
    //{
    //    List<CreateMaintainItemModel> MaintainItemList { get; set; } // 保養項目
    //}

    public interface IUpdateMaintainItemList
    {
        string TSN { get; set; } // 一機一卡模板編號
        IEnumerable<string> MINameList { get; set; } // 保養項目名稱列表
    }
    #endregion

    #region CheckItem 檢查項目
    public interface ICheckItemName
    {
        string Value { get; set; } // 檢查項目名稱
    }

    public class CreateCheckItemModel : ICheckItemName
    {
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "檢查項目名稱")]
        public string Value { get; set; } // 檢查項目名稱
    }

    //public interface ICreateCheckItemList
    //{
    //    List<CreateCheckItemModel> CheckItemList { get; set; } // 檢查項目名稱
    //}

    public interface IUpdateCheckItemList
    {
        string TSN { get; set; } // 一機一卡模板編號
        int? Frequency { get; set; } // 巡檢頻率
        IEnumerable<string> CINameList { get; set; } // 檢查項目名稱列表
    }
    #endregion

    #region ReportItem 填報項目
    public interface IReportItem
    {
        string Value { get; set; } // 填報項目名稱
        string Unit { get; set; } // 填報項目單位
    }

    public class CreateReportItemModel : IReportItem
    {
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "填報項目名稱")]
        public string Value { get; set; } // 填報項目名稱
        [Required]
        [StringLength(20, ErrorMessage = "{0} 的長度最多20個字元。")]
        [Display(Name = "單位")]
        public string Unit { get; set; } // 填報項目單位
    }

    //public interface ICreateReportItemList
    //{
    //    List<CreateReportItemModel> ReportItemList { get; set; } // 填報項目
    //}

    public class UpdateReportItemModel
    {

        public string RIName { get; set; } // 填報項目名稱

        public string Unit { get; set; } // 單位
    }

    public interface IUpdateReportItemList
    {
        string TSN { get; set; } // 一機一卡模板編號
        int? Frequency { get; set; } // 巡檢頻率
        IEnumerable<UpdateReportItemModel> RIList { get; set; } // 填報項目
    }
    #endregion
}