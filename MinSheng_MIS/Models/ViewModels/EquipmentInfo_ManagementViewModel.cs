using MinSheng_MIS.Attributes;
using MinSheng_MIS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    #region 設備-新增
    /// <summary>
    /// 新增設備DTO
    /// </summary>
    public class EquipmentInfoCreateViewModel : EquipInfo, 
        ICreateEquipmentInfo, ICreateAddFieldValueList, ICreateMaintainItemValueList
    {
        public string TSN { get; set; } // 一機一卡模板編號
        [Required]
        [FileSizeLimit(10)] // 限制大小為 10 MB
        [Display(Name = "設備照片")]
        public HttpPostedFileBase EPhoto { get; set; } //新增的照片
        public List<AddFieldValueModel> AddFieldList { get; set; } // 一機一卡模板資料：增設基本資料欄位
        public List<MaintainItemValueModel> MaintainItemList { get; set; } // 一機一卡模板資料：保養項目設定

        internal IUpdateAddFieldValue ConvertToUpdateAddFieldValue(string esn)
        {
            return new UpdateAddFieldValueInstance
            {
                ESN = esn,
                TSN = this.TSN,
                AddFieldList = this.AddFieldList
            };
        }

        internal IUpdateMaintainItemValue ConvertToUpdateMaintainItemValue(string esn)
        {
            return new UpdateMaintainItemValueInstance
            {
                ESN = esn,
                TSN = this.TSN,
                MaintainItemList = this.MaintainItemList
            };
        }
    }

    /// <summary>
    /// 新增設備資料所需資訊
    /// </summary>
    public interface ICreateEquipmentInfo : IEquipmentInfo
    {
        string TSN { get; set; } // 一機一卡模板編號
        HttpPostedFileBase EPhoto { get; set; } //新增的設備照片
    }

    /// <summary>
    /// 使用者變更設備中一機一卡增設欄位值所需資訊
    /// </summary>
    public interface ICreateAddFieldValueList
    {
        string TSN { get; set; } // 一機一卡模板編號(用於資料驗證)
        List<AddFieldValueModel> AddFieldList { get; set; } // 增設基本資料欄位
    }

    /// <summary>
    /// 使用者新增設備中一機一卡保養資訊的週期及下次保養日期所需資訊
    /// </summary>
    public interface ICreateMaintainItemValueList
    {
        string TSN { get; set; } // 一機一卡模板編號(用於資料驗證)
        List<MaintainItemValueModel> MaintainItemList { get; set; } // 保養資訊設定
    }
    #endregion

    #region 設備-詳情
    /// <summary>
    /// 設備資料詳情
    /// </summary>
    public class EquipmentInfoDetailViewModel : EquipmentInfoDetailModel, IDeviceCardDetail
    {
        public string TSN { get; set; } // 一機一卡模板編號
        public string SampleName { get; set; } // 一機一卡模板名稱
        public int? Frequency { get; set; } // 巡檢頻率
        public new IEnumerable<IRFIDInfoDetail> RFIDList { get; set; } // RFID
        public IEnumerable<IAddFieldValueDetail> AddFieldList { get; set; } // 一機一卡模板資料：增設基本資料欄位
        public IEnumerable<IMaintainItemValueDetail> MaintainItemList { get; set; } // 一機一卡模板資料：保養項目設定
        public IEnumerable<ICheckItemDetail> CheckItemList { get; set; } // 一機一卡模板資料：檢查項目
        public IEnumerable<IReportItemDetail> ReportItemList { get; set; } // 一機一卡模板資料：填報項目名稱/單位
    }

    public class EquipmentInfoDetailModel : EquipInfo, IEquipmentInfoDetail
    {
        public new string InstallDate { get; set; } // 安裝日期
        public string ESN { get; set; } // 設備資料(EquipmentInfo)編號
        public string FilePath { get; set; } // 設備照片路徑
        public string FileName { get; set; } // 設備照片名稱
        public string EState { get; set; } // 設備狀態
        public string ASN { get; set; } // 棟別
        public string AreaName { get; set; } // 棟別名稱
        public string FloorName { get; set; } // 樓層名稱
        public string GUID { get; set; } // GUID
    }

    public interface IEquipmentInfoDetail : IEquipmentInfo
    {
        new string InstallDate { get; set; } // 安裝日期

        string ESN { get; set; } // 設備資料(EquipmentInfo)編號
        string FilePath { get; set; } // 設備照片路徑
        string FileName { get; set; } // 設備照片名稱
        string EState { get; set; } // 設備狀態
        string ASN { get; set; } // 棟別
        string AreaName { get; set; } // 棟別名稱
        string FloorName { get; set; } // 樓層名稱
        string GUID { get; set; } // GUID
    }

    public class AddFieldValueDetail : AddFieldValueModel, IAddFieldValueDetail
    {
        public string Text { get; set; } // 欄位名稱
    }

    public interface IAddFieldValueDetail : IAddFieldValue
    {
        string Text { get; set; } // 欄位名稱
    }

    public class MaintainItemValueDetail : MaintainItemValueModel, IMaintainItemValueDetail
    {
        public string Text { get; set; } // 保養項目名稱
        public string PeriodText { get; set; } // 週期名稱
        public string LastMaintainDate { get; set; } // 上次保養日期
        public new string NextMaintainDate { get; set; } // 下次保養日期
    }

    public interface IMaintainItemValueDetail : IMaintainItemValue
    {
        string Text { get; set; } // 保養項目名稱
        string PeriodText { get; set; } // 週期名稱
        string LastMaintainDate { get; set; } // 上次保養日期
    }
    #endregion

    #region 設備-編輯
    /// <summary>
    /// 編輯設備DTO
    /// </summary>
    public class EquipmentInfoEditViewModel : EquipInfo,
        IEditEquipmentInfo, IEditAddFieldValueList, IEditMaintainItemValueList
    {
        string IUpdateEquipmentInfo.EState { get; set; } // 設備狀態
        [Required]
        [StringLength(12, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "設備資料編號")]
        public string ESN { get; set; } // 設備資料(EquipmentInfo)編號
        public string TSN { get; set; } // 一機一卡模板編號
        [FileSizeLimit(10)] // 限制大小為 10 MB
        [Display(Name = "設備照片")]
        public HttpPostedFileBase EPhoto { get; set; } //新增的照片
        public string FileName { get; set; } // 設備原照片名稱
        public List<AddFieldValueModel> AddFieldList { get; set; } // 一機一卡模板資料：增設基本資料欄位
        public List<MaintainItemValueModel> MaintainItemList { get; set; } // 一機一卡模板資料：保養項目設定
    }

    /// <summary>
    /// 編輯設備資料所需資訊
    /// </summary>
    public interface IEditEquipmentInfo : ICreateEquipmentInfo, IUpdateEquipmentInfo
    {
        string FileName { get; set; } // 設備原照片名稱
    }

    /// <summary>
    /// 使用者變更設備中一機一卡增設欄位值所需資訊
    /// </summary>
    public interface IEditAddFieldValueList : 
        IUpdateAddFieldValue, ICreateAddFieldValueList { }

    /// <summary>
    /// 使用者新增設備中一機一卡保養資訊的週期及下次保養日期所需資訊
    /// </summary>
    public interface IEditMaintainItemValueList : 
        IUpdateMaintainItemValue, ICreateMaintainItemValueList { }
    #endregion

    #region Shared
    public interface IEquipmentName
    {
        string EName { get; set; } // 設備名稱
        string NO { get; set; } // 設備編號
    }

    /// <summary>
    /// 設備資料可變更之資訊
    /// </summary>
    public interface IEquipmentInfo : IEquipmentName
    {
        DateTime? InstallDate { get; set; } // 安裝日期
        string FSN { get; set; } // 樓層編號
        string Brand { get; set; } // 設備廠牌
        string Model { get; set; } // 設備型號
        string Vendor { get; set; } // 設備廠商
        string ContactPhone { get; set; } // 連絡電話
        string OperatingVoltage { get; set; } // 使用電壓
        string OtherInfo { get; set; } // 其他耗材資料
        string Memo { get; set; } // 備註
    }

    public abstract class EquipInfo : IEquipmentInfo
    {
        //-----Implement IUpdateEquipmentInfo
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "設備名稱")]
        public string EName { get; set; } // 設備名稱
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "設備編號")]
        public string NO { get; set; } // 設備編號
        public DateTime? InstallDate { get; set; } // 安裝日期
        [Required]
        [StringLength(5, ErrorMessage = "{0} 的長度最多5個字元。")]
        [Display(Name = "樓層編號")]
        public string FSN { get; set; } // 樓層編號
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "設備廠牌")]
        public string Brand { get; set; } // 設備廠牌
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "設備型號")]
        public string Model { get; set; } // 設備型號
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "設備廠商")]
        public string Vendor { get; set; } // 設備廠商
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "連絡電話")]
        public string ContactPhone { get; set; } // 連絡電話
        [StringLength(20, ErrorMessage = "{0} 的長度最多20個字元。")]
        [Display(Name = "使用電壓")]
        public string OperatingVoltage { get; set; } // 使用電壓
        [StringLength(200, ErrorMessage = "{0} 的長度最多200個字元。")]
        [Display(Name = "其他耗材資料")]
        public string OtherInfo { get; set; } // 其他耗材資料
        [StringLength(200, ErrorMessage = "{0} 的長度最多200個字元。")]
        [Display(Name = "備註")]
        public string Memo { get; set; } // 備註
        public List<CreateEquipRFID> RFIDList { get; set; } // 設備RFID

        internal IUpdateRFID ConvertToUpdateRFID(string esn)
        {
            return new UpdateRFIDInstance
            {
                ESN = esn,
                RFIDList = RFIDList.Select(x =>
                {
                    var temp = x.ToDto<CreateEquipRFID, EditEquipRFID>();
                    temp.ESN = esn;
                    return temp;
                }).ToList()
            };
        }
    }

    /// <summary>
    /// 一機一卡增設欄位值資訊
    /// </summary>
    public interface IAddFieldValue
    {
        string AFSN { get; set; } // 模板增設欄位編號
        string Value { get; set; } // 欄位值
    }

    public class AddFieldValueModel : IAddFieldValue
    {
        [Required]
        [StringLength(11, ErrorMessage = "{0} 的長度最多11個字元。")]
        [Display(Name = "模板增設欄位編號")]
        public string AFSN { get; set; } // 模板增設欄位編號
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "欄位值")]
        public string Value { get; set; } // 欄位值
    }

    /// <summary>
    /// 一機一卡保養資訊之週期及下次保養日期
    /// </summary>
    public interface IMaintainItemValue
    {
        string MISSN { get; set; } // 模板保養項目編號
        string Period { get; set; } // 週期
        DateTime NextMaintainDate { get; set; } // 下次保養日期
    }

    public class MaintainItemValueModel : IMaintainItemValue
    {
        [Required]
        [StringLength(11, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "模板保養項目編號")]
        public string MISSN { get; set; } // 模板保養項目編號
        [Required]
        [StringLength(1, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "週期")]
        public string Period { get; set; } // 週期
        [Required]
        [Display(Name = "下次保養日期")]
        public DateTime NextMaintainDate { get; set; } // 下次保養日期
    }
    #endregion

    /// <summary>
    /// 變更設備資料所需資訊
    /// </summary>
    public interface IUpdateEquipmentInfo : ICreateEquipmentInfo
    {
        string ESN { get; set; } // 設備資料(EquipmentInfo)編號
        string EState { get; set; } // 設備狀態
    }

    /// <summary>
    /// 系統變更設備中一機一卡增設欄位值所需資訊
    /// </summary>
    public interface IUpdateAddFieldValue : ICreateAddFieldValueList
    {
        string ESN { get; set; } // 設備資料(EquipmentInfo)編號
    }

    /// <summary>
    /// 系統變更設備中一機一卡保養資訊的週期及下次保養日期所需資訊
    /// </summary>
    public interface IUpdateMaintainItemValue : ICreateMaintainItemValueList
    {
        string ESN { get; set; } // 設備資料(EquipmentInfo)編號
    }

    public interface IUpdateRFID
    {
        string ESN { get; set; } // 設備資料(EquipmentInfo)編號
        List<EditEquipRFID> RFIDList { get; set; }
    }

    #region Service需要的實例
    public class UpdateEquipmentInfoInstance : EquipInfo, IUpdateEquipmentInfo
    {
        public string ESN { get; set; }
        public string TSN { get; set; }
        public string EState { get; set; }
        public HttpPostedFileBase EPhoto { get; set; } //新增的照片

        public new List<CreateEquipRFID> RFIDList { get; }
    }

    public class UpdateAddFieldValueInstance : IUpdateAddFieldValue
    {
        public string ESN { get; set; }
        public string TSN { get; set; }
        public List<AddFieldValueModel> AddFieldList { get; set; }
    }
    public class UpdateMaintainItemValueInstance : IUpdateMaintainItemValue
    {
        public string ESN { get; set; }
        public string TSN { get; set; }
        public List<MaintainItemValueModel> MaintainItemList { get; set; }
    }

    public class UpdateRFIDInstance : IUpdateRFID
    {
        public string ESN { get; set; }
        public List<EditEquipRFID> RFIDList { get; set; }
    }
    #endregion
}