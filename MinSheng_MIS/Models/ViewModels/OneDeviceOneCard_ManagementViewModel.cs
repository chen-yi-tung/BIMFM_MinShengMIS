using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MinSheng_MIS.Models.ViewModels
{
    #region 一機一卡-新增
    public class DeviceCardCreateViewModel : ICreateDeviceCard
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
    }

    public class AddFieldNameModel : IAddFieldName
    {
        [Required]
        [StringLength(20, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "增設基本資料欄位名稱")]
        public string Value { get; set; } // 欄位名稱
    }

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

    /// <summary>
    /// 增設基本資料欄位
    /// </summary>
    public class AddFieldDetailModel : IAddFieldDetail
    {
        public string AFSN { get; set; } // 模板增設欄位編號
        [Required]
        public string Value { get; set; } // 欄位名稱
    }

    /// <summary>
    /// 保養資訊
    /// </summary>
    public class MaintainItemDetailModel : IMaintainItemDetail
    {
        public string MISSN { get; set; } // 模板保養編號
        [Required]
        public string Value { get; set; } // 保養項目名稱
    }

    /// <summary>
    /// 巡檢資訊-檢查項目
    /// </summary>
    public class CheckItemDetailModel : ICheckItemDetail
    {
        public string CISN { get; set; } // 模板檢查項目編號
        [Required]
        public string Value { get; set; } // 檢查項目名稱
    }

    /// <summary>
    /// 巡檢資訊-填報項目
    /// </summary>
    public class ReportItemDetailModel : IReportItemDetail
    {
        public string RISN { get; set; } // 模板填報項目編號
        [Required]
        public string Value { get; set; } // 填報項目名稱
        [Required]
        public string Unit { get; set; } // 填報項目單位
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
        public string TSN { get; set; } // 一機一卡模板編號
        public string SampleName { get; set; } // 模板名稱
        public int? Frequency { get; set; } // 巡檢頻率
        public List<AddFieldDetailModel> AddItemList { get; set; } // 增設基本資料欄位
        public List<MaintainItemDetailModel> MaintainItemList { get; set; } // 既有保養項目/未有設備使用之保養項目
        public List<AddEquipmentUsedMaintainItem> AddMaintainItemList { get; set; } // 新增之保養項目及其於各設備值
        public List<CheckItemDetailModel> CheckItemList { get; set; } // 檢查項目列表
        public List<ReportItemDetailModel> ReportItemList { get; set; } // 填報項目列表
    }
    #endregion

    #region 一機一卡-刪除
    public class DeleteAddFieldList : IDeleteAddFieldList
    {
        public IEnumerable<string> AFSN { get; set; }

        public DeleteAddFieldList() { }

        public DeleteAddFieldList(Template_OneDeviceOneCard temp)
        {
            AFSN = temp.Template_AddField.Select(x => x.AFSN);
        }
    }

    public class DeleteMaintainItemList : IDeleteMaintainItemList
    {
        public IEnumerable<string> MISSN { get; set; }

        public DeleteMaintainItemList() { }

        public DeleteMaintainItemList(Template_OneDeviceOneCard temp)
        {
            MISSN = temp.Template_MaintainItemSetting.Select(x => x.MISSN);
        }
    }

    public class DeleteCheckItemList : IDeleteCheckItemList
    {
        public IEnumerable<string> CISN { get; set; }

        public DeleteCheckItemList() { }

        public DeleteCheckItemList(Template_OneDeviceOneCard temp)
        {
            CISN = temp.Template_CheckItem.Select(x => x.CISN);
        }
    }

    public class DeleteReportItemList : IDeleteReportItemList
    {
        public IEnumerable<string> RISN { get; set; }

        public DeleteReportItemList() { }

        public DeleteReportItemList(Template_OneDeviceOneCard temp)
        {
            RISN = temp.Template_ReportingItem.Select(x => x.RISN);
        }
    }
    #endregion

    //-----Interface & Abstract class
    #region OneDeviceOneCard 一機一卡
    public interface IDeviceCard
    {
        string SampleName { get; set; } // 模板名稱
        int? Frequency { get; set; } // 巡檢頻率
    }

    public interface IDeviceCardDetail : IDeviceCard
    {
        string TSN { get; set; } // 一機一卡模板編號
    }

    public interface ICreateDeviceCard : IDeviceCard { }

    public interface IUpdateDeviceCard : IDeviceCardDetail, ICreateDeviceCard { }
    #endregion

    #region AddField 增設基本資料欄位
    public interface IAddFieldName
    {
        string Value { get; set; } // 欄位名稱
    }

    public interface IAddFieldDetail : IAddFieldName
    {
        string AFSN { get; set; } // 模板增設欄位編號
    }

    public interface ICreateAddFieldList : IAddFieldModifiableList { }

    public interface IUpdateAddFieldList
    {
        string TSN { get; set; }
        List<AddFieldDetailModel> AddItemList { get; set; }
    }

    public interface IDeleteAddFieldList
    {
        IEnumerable<string> AFSN { get; set; } // 模板增設欄位編號
    }
    #endregion

    #region MaintainItem 保養項目
    public interface IMaintainItemName
    {
        string Value { get; set; } // 保養項目名稱
    }

    public interface IMaintainItemDetail : IMaintainItemName
    {
        string MISSN { get; set; } // 模板保養編號
    }

    public interface ICreateMaintainItemList : IMaintainItemModifiableList { }

    public interface IUpdateMaintainItemList
    {
        string TSN { get; set; }
        List<MaintainItemDetailModel> MaintainItemList { get; set; } // 既有保養項目/未有設備使用之保養項目
        List<AddEquipmentUsedMaintainItem> AddMaintainItemList { get; set; } // 新增之保養項目及其於各設備值
    }

    public class AddEquipmentUsedMaintainItem
    {
        public string ESN { get; set; } // 設備資料(EquipmentInfo)編號
        public string MaintainName { get; set; } // 保養項目名稱
        public string Period { get; set; } // 週期
        public DateTime NextMaintainDate { get; set; } // 下次保養日期
    }

    public interface IDeleteMaintainItemList
    {
        IEnumerable<string> MISSN { get; set; } // 模板保養編號
    }
    #endregion

    #region CheckItem 檢查項目
    public interface ICheckItemName
    {
        string Value { get; set; } // 檢查項目名稱
    }

    public interface ICheckItemDetail : ICheckItemName
    {
        string CISN { get; set; } // 模板檢查項目編號
    }

    public interface ICreateCheckItemList : ICheckItemModifiableList { }

    public interface IUpdateCheckItemList
    {
        string TSN { get; set; }
        int? Frequency { get; set; } // 巡檢頻率
        List<CheckItemDetailModel> CheckItemList { get; set; } // 檢查項目名稱列表
    }

    public interface IDeleteCheckItemList
    {
        IEnumerable<string> CISN { get; set; } // 模板檢查項目編號
    }
    #endregion

    #region ReportItem 填報項目
    public interface IReportItem
    {
        string Value { get; set; } // 填報項目名稱
        string Unit { get; set; } // 填報項目單位
    }

    public interface IReportItemDetail : IReportItem
    {
        string RISN { get; set; } // 模板填報項目編號
    }

    public class UpdateReportItemModel
    {
        public string RIName { get; set; } // 填報項目名稱

        public string Unit { get; set; } // 單位
    }

    public interface ICreateReportItemList : IReportItemModifiableList { }

    public interface IUpdateReportItemList
    {
        string TSN { get; set; }
        int? Frequency { get; set; } // 巡檢頻率
        List<ReportItemDetailModel> ReportItemList { get; set; } // 填報項目列表
    }

    public interface IDeleteReportItemList
    {
        IEnumerable<string> RISN { get; set; } // 模板填報項目編號
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

    public class AddFieldModifiableListInstance: IAddFieldModifiableList, ICreateAddFieldList
    {
        public string TSN { get; set; }
        public IEnumerable<string> AFNameList { get; set; } // 增設基本資料欄位

        public AddFieldModifiableListInstance(string tsn, DeviceCardCreateViewModel data)
        {
            TSN = tsn;
            AFNameList = data.AddItemList?.Select(x => x.Value).AsEnumerable();
        }

        public AddFieldModifiableListInstance(in DeviceCardEditViewModel data, 
            bool onlyEmptyAfsn = false)
        {
            var list = data.AddItemList?.ToList();
            if (onlyEmptyAfsn)
                list?.RemoveAll(x => !string.IsNullOrEmpty(x.AFSN));

            TSN = data.TSN;
            AFNameList = list?.Select(x => x.Value).AsEnumerable();
        }
    }

    /// <summary>
    /// 保養項目可變動資料
    /// </summary>
    public interface IMaintainItemModifiableList
    {
        string TSN { get; set; } // 一機一卡模板編號
        IEnumerable<string> MINameList { get; set; } // 保養項目名稱列表
    }

    public class MaintainItemModifiableListInstance : IMaintainItemModifiableList, ICreateMaintainItemList
    {
        public string TSN { get; set; }
        public IEnumerable<string> MINameList { get; set; } // 保養項目名稱列表

        public MaintainItemModifiableListInstance(string tsn, DeviceCardCreateViewModel data)
        {
            TSN = tsn;
            MINameList = data.MaintainItemList?.Select(x => x.Value).AsEnumerable();
        }

        public MaintainItemModifiableListInstance(in DeviceCardEditViewModel data, 
            bool onlyEmptyMISSN = false, bool noEquipmentUsed = false, bool equipmentUsed = false)
        {
            TSN = data.TSN;

            var temp = new List<string>();
            var list = data.MaintainItemList?.ToList();
            if (onlyEmptyMISSN)
                list?.RemoveAll(x => !string.IsNullOrEmpty(x.MISSN));
            if (noEquipmentUsed)
                temp = list?.Select(x => x.Value).ToList();
            if (equipmentUsed && data.AddMaintainItemList != null)
                temp.AddRange(data.AddMaintainItemList?.Select(x => x.MaintainName).Distinct());

            MINameList = temp;
        }
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

    public class CheckItemModifiableListInstance : ICheckItemModifiableList, ICreateCheckItemList
    {
        public string TSN { get; set; }
        public int? Frequency { get; set; }
        public IEnumerable<string> CINameList { get; set; } // 檢查項目名稱列表

        public CheckItemModifiableListInstance(string tsn, DeviceCardCreateViewModel data)
        {
            TSN = tsn;
            Frequency = data.Frequency;
            CINameList = data.CheckItemList?.Select(x => x.Value).AsEnumerable();
        }

        public CheckItemModifiableListInstance(in DeviceCardEditViewModel data,
            bool onlyEmptyCisn = false)
        {
            var list = data.CheckItemList?.ToList();
            if (onlyEmptyCisn)
                list?.RemoveAll(x => !string.IsNullOrEmpty(x.CISN));

            TSN = data.TSN;
            Frequency = data.Frequency;
            CINameList = list?.Select(x => x.Value).AsEnumerable();
        }
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
    public class ReportItemModifiableListInstance : IReportItemModifiableList, ICreateReportItemList
    {
        public string TSN { get; set; }
        public int? Frequency { get; set; }
        public IEnumerable<UpdateReportItemModel> RIList { get; set; } // 填報項目列表

        public ReportItemModifiableListInstance(string tsn, DeviceCardCreateViewModel data)
        {
            TSN = tsn;
            Frequency = data.Frequency;
            RIList = data.ReportItemList?.Select(x => new UpdateReportItemModel {
                RIName = x.Value,
                Unit = x.Unit,
            }).AsEnumerable();
        }

        public ReportItemModifiableListInstance(in DeviceCardEditViewModel data,
            bool onlyEmptyRisn = false)
        {
            var list = data.ReportItemList?.ToList();
            if (onlyEmptyRisn)
                list?.RemoveAll(x => !string.IsNullOrEmpty(x.RISN));

            TSN = data.TSN;
            Frequency = data.Frequency;
            RIList = list?.Select(x => new UpdateReportItemModel
            {
                RIName = x.Value,
                Unit = x.Unit,
            }).AsEnumerable();
        }
    }
    #endregion
}