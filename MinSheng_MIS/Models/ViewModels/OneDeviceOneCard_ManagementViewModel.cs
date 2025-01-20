using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MinSheng_MIS.Models.ViewModels
{
    #region 一機一卡-新增
    public class DeviceCardCreateViewModel : ICreateDeviceCard,
        ICreateAddFieldList, ICreateMaintainItemList, ICreateCheckItemList, ICreateReportItemList
    {
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "模板名稱")]
        public virtual string SampleName { get; set; } // 模板名稱
        public int? Frequency { get; set; } // 巡檢頻率
        public List<AddFieldNameModel> AddItemList { get; set; } // 增設基本資料欄位
        public List<MaintainItemNameModel> MaintainItemList { get; set; } // 保養項目
        public List<CheckItemNameModel> CheckItemList { get; set; } // 檢查項目
        public List<ReportItemModel> ReportItemList { get; set; } // 填報項目名稱/單位

        private string TSN { get; set; }
        string IAddFieldModifiableList.TSN { get => TSN; set => TSN = value; }
        string IMaintainItemModifiableList.TSN { get => TSN; set => TSN = value; }
        string ICheckItemModifiableList.TSN { get => TSN; set => TSN = value; }
        string IReportItemModifiableList.TSN { get => TSN; set => TSN = value; }

        IEnumerable<string> IAddFieldModifiableList.AFNameList { get; set; }
        IEnumerable<string> IMaintainItemModifiableList.MINameList { get; set; }
        IEnumerable<string> ICheckItemModifiableList.CINameList { get; set; }
        IEnumerable<UpdateReportItemModel> IReportItemModifiableList.RIList { get; set; }

        internal void SetTsn(string tsn)
        {
            TSN = tsn;
        }

        internal void SetServiceList()
        {
            ((IAddFieldModifiableList)this).AFNameList = AddItemList?.Select(x => x.Value).AsEnumerable();
            ((IMaintainItemModifiableList)this).MINameList = MaintainItemList?.Select(x => x.Value).AsEnumerable();
            ((ICheckItemModifiableList)this).CINameList = CheckItemList?.Select(x => x.Value).AsEnumerable();
            ((IReportItemModifiableList)this).RIList = ReportItemList?
                .Select(x => new UpdateReportItemModel
                {
                    RIName = x.Value,
                    Unit = x.Unit,
                })
                .AsEnumerable();
        }
    }

    public interface ICreateDeviceCard : IDeviceCard { }
    public interface ICreateAddFieldList : IAddFieldModifiableList { }
    public interface ICreateMaintainItemList : IMaintainItemModifiableList { }
    public interface ICreateCheckItemList : ICheckItemModifiableList { }
    public interface ICreateReportItemList : IReportItemModifiableList { }

    public class MaintainItemNameModel : IMaintainItemName
    {
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "保養項目名稱")]
        public string Value { get; set; } // 保養項目名稱
    }

    public class CheckItemNameModel : ICheckItemName
    {
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "檢查項目名稱")]
        public string Value { get; set; } // 檢查項目名稱
    }

    public class ReportItemModel : IReportItem
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
    #endregion

    #region 一機一卡-詳情
    public class DeviceCardDetailViewModel : IDeviceCardDetail
    {
        public string TSN { get; set; } // 一機一卡模板編號
        public string SampleName { get; set; } // 模板名稱
        public int? Frequency { get; set; } // 巡檢頻率
        public List<IAddFieldDetail> AddItemList { get; set; } // 增設基本資料欄位
        public List<IMaintainItemDetail> MaintainItemList { get; set; }  // 保養項目
        public List<ICheckItemDetail> CheckItemList { get; set; } // 檢查項目
        public List<IReportItemDetail> ReportItemList { get; set; } // 填報項目名稱/單位
    }
    #endregion

    #region 一機一卡-編輯
    public class DeviceCardEditViewModel : 
        IUpdateDeviceCard, 
        IUpdateAddFieldList, 
        IUpdateMaintainItemList, 
        IUpdateCheckItemList,
        IUpdateReportItemList
    {
        [Required]
        [StringLength(8, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "一機一卡模板編號")]
        public string TSN { get; set; } // 一機一卡模板編號
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "模板名稱")]
        public string SampleName { get; set; } // 模板名稱
        public int? Frequency { get; set; } // 巡檢頻率
        public List<AddFieldDetailModel> AddItemList { get; set; } // 增設基本資料欄位
        public List<MaintainItemDetailModel> MaintainItemList { get; set; } // 既有保養項目/未有設備使用之保養項目
        public List<AddEquipmentUsedMaintainItem> AddMaintainItemList { get; set; } // 新增之保養項目及其於各設備值
        public List<CheckItemDetailModel> CheckItemList { get; set; } // 檢查項目列表
        public List<ReportItemDetailModel> ReportItemList { get; set; } // 填報項目列表

        IEnumerable<string> IAddFieldModifiableList.AFNameList { get; set; } // 增設基本資料欄位名稱列表
        IEnumerable<string> IMaintainItemModifiableList.MINameList { get; set; } // 保養項目名稱列表
        IEnumerable<string> ICheckItemModifiableList.CINameList { get; set; } // 檢查項目名稱列表
        IEnumerable<UpdateReportItemModel> IReportItemModifiableList.RIList { get; set; } // 填報項目列表

        public void SetAddFieldModifiableList(bool onlyEmptyAfsn = false)
        {
            var temp = AddItemList.AsEnumerable();
            if (onlyEmptyAfsn) // 篩出新的增設欄位
                temp = temp?.Where(x => string.IsNullOrEmpty(x.AFSN));
            ((IAddFieldModifiableList)this).AFNameList = temp?.Select(x => x.Value) ?? new List<string>();
        }

        public void SetMaintainItemModifiableList(bool onlyEmptyMISSN = false)
        {
            var temp = MaintainItemList?.AsEnumerable();
            if (onlyEmptyMISSN) // 篩出新的保養項目
                temp = temp?.Where(x => string.IsNullOrEmpty(x.MISSN));
            ((IMaintainItemModifiableList)this).MINameList = temp?.Select(x => x.Value) ?? new List<string>();
        }

        public void SetCheckItemModifiableList(bool onlyEmptyCisn = false)
        {
            var temp = CheckItemList?.AsEnumerable();
            if (onlyEmptyCisn) // 篩出新的檢查項目
                temp = temp?.Where(x => string.IsNullOrEmpty(x.CISN));
            ((ICheckItemModifiableList)this).CINameList = temp?.Select(x => x.Value) ?? new List<string>();
        }

        public void SetReportItemModifiableList(bool onlyEmptyRisn = false)
        {
            var temp = ReportItemList?.AsEnumerable();
            if (onlyEmptyRisn) // 篩出新的填報項目
                temp = temp?.Where(x => string.IsNullOrEmpty(x.RISN));
            ((IReportItemModifiableList)this).RIList = temp?.Select(x => new UpdateReportItemModel
                {
                    RIName = x.Value,
                    Unit = x.Unit,
                }) ?? new List<UpdateReportItemModel>();
        }
    }

    public class AddEquipmentUsedMaintainItem
    {
        [Required]
        [StringLength(12, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "設備資料編號")]
        public string ESN { get; set; } // 設備資料(EquipmentInfo)編號
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "保養項目名稱")]
        public string MaintainName { get; set; } // 保養項目名稱
        [Required]
        [StringLength(1, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "週期")]
        public string Period { get; set; } // 週期
        [Required]
        [Display(Name = "下次保養日期")]
        public DateTime NextMaintainDate { get; set; } // 下次保養日期
    }

    public interface IUpdateDeviceCard : IDeviceCardDetail, ICreateDeviceCard { }

    public interface IUpdateAddFieldList : ICreateAddFieldList
    {
        List<AddFieldDetailModel> AddItemList { get; set; }

        void SetAddFieldModifiableList(bool onlyEmptyAfsn = false);
    }

    public interface IUpdateMaintainItemList : ICreateMaintainItemList
    {
        List<MaintainItemDetailModel> MaintainItemList { get; set; } // 既有保養項目/未有設備使用之保養項目
        List<AddEquipmentUsedMaintainItem> AddMaintainItemList { get; set; } // 新增之保養項目及其於各設備值

        void SetMaintainItemModifiableList(bool onlyEmptyMISSN = false);
    }

    public interface IUpdateCheckItemList : ICreateCheckItemList
    {
        List<CheckItemDetailModel> CheckItemList { get; set; } // 檢查項目名稱列表

        void SetCheckItemModifiableList(bool onlyEmptyCisn = false);
    }

    public interface IUpdateReportItemList : ICreateReportItemList
    {
        List<ReportItemDetailModel> ReportItemList { get; set; } // 填報項目列表

        void SetReportItemModifiableList(bool onlyEmptyRisn = false);
    }
    #endregion

    #region Shared
    // ----- 一機一卡
    public interface IDeviceCard
    {
        string SampleName { get; set; } // 模板名稱
        int? Frequency { get; set; } // 巡檢頻率
    }

    public interface IDeviceCardDetail : IDeviceCard
    {
        string TSN { get; set; } // 一機一卡模板編號
    }

    // ----- 增設基本資料欄位
    public class AddFieldDetailModel : AddFieldNameModel, IAddFieldDetail
    {
        public string AFSN { get; set; } // 模板增設欄位編號
    }

    public class AddFieldNameModel : IAddFieldName
    {
        [Required]
        [StringLength(20, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "增設基本資料欄位名稱")]
        public string Value { get; set; } // 欄位名稱
    }

    public interface IAddFieldName
    {
        string Value { get; set; } // 欄位名稱
    }

    public interface IAddFieldDetail : IAddFieldName
    {
        string AFSN { get; set; } // 模板增設欄位編號
    }

    // ----- 保養資訊
    public class MaintainItemDetailModel : MaintainItemNameModel, IMaintainItemDetail
    {
        public string MISSN { get; set; } // 模板保養編號
    }
    
    public interface IMaintainItemName
    {
        string Value { get; set; } // 保養項目名稱
    }

    public interface IMaintainItemDetail : IMaintainItemName
    {
        string MISSN { get; set; } // 模板保養編號
    }

    // ----- 巡檢資訊-檢查項目
    public class CheckItemDetailModel : CheckItemNameModel, ICheckItemDetail
    {
        public string CISN { get; set; } // 模板檢查項目編號
    }

    public interface ICheckItemName
    {
        string Value { get; set; } // 檢查項目名稱
    }

    public interface ICheckItemDetail : ICheckItemName
    {
        string CISN { get; set; } // 模板檢查項目編號
    }

    // ----- 巡檢資訊-填報項目
    public class ReportItemDetailModel : ReportItemModel, IReportItemDetail
    {
        public string RISN { get; set; } // 模板填報項目編號
    }

    public interface IReportItem
    {
        string Value { get; set; } // 填報項目名稱
        string Unit { get; set; } // 填報項目單位
    }

    public interface IReportItemDetail : IReportItem
    {
        string RISN { get; set; } // 模板填報項目編號
    }
    #endregion

    #region Service使用
    /// <summary>
    /// 增設基本資料欄位可變動資料
    /// </summary>
    public interface IAddFieldModifiableList
    {
        string TSN { get; set; } // 一機一卡模板編號
        IEnumerable<string> AFNameList { get; set; } // 增設基本資料欄位名稱列表
    }

    /// <summary>
    /// 保養項目可變動資料
    /// </summary>
    public interface IMaintainItemModifiableList
    {
        string TSN { get; set; } // 一機一卡模板編號
        IEnumerable<string> MINameList { get; set; } // 保養項目名稱列表
    }

    /// <summary>
    /// 檢查項目可變動資料
    /// </summary>
    public interface ICheckItemModifiableList
    {
        string TSN { get; set; } // 一機一卡模板編號
        int? Frequency { get; set; } // 巡檢頻率
        IEnumerable<string> CINameList { get; set; } // 檢查項目名稱列表
    }

    /// <summary>
    /// 檢查項目可變動資料
    /// </summary>
    public interface IReportItemModifiableList
    {
        string TSN { get; set; } // 一機一卡模板編號
        int? Frequency { get; set; } // 巡檢頻率
        IEnumerable<UpdateReportItemModel> RIList { get; set; } // 填報項目列表
    }
    public class UpdateReportItemModel
    {
        public string RIName { get; set; } // 填報項目名稱

        public string Unit { get; set; } // 單位
    }
    #endregion
}