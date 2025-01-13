using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MinSheng_MIS.Models.ViewModels
{
    #region 巡檢路線模板-新增
    public class SamplePathCreateViewModel : ICreateSamplePath, IEditSamplePath
    {
        string IDefaultOrderModifiableList.PlanPathSN { get; set; }
        string IEditSamplePathInfo.PlanPathSN { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "巡檢路線名稱")]
        public string PathName { get; set; } // 巡檢路線名稱
        [Required]
        [Display(Name = "巡檢頻率")]
        public int Frequency { get; set; } // 巡檢頻率
        [Required]
        [Display(Name = "巡檢設備")]
        public IEnumerable<string> RFIDInternalCodes { get; set; } // 巡檢設備RFID清單
    }

    public interface ICreateSamplePath :
        ICreateSamplePathInfo, IDefaultOrderModifiableList { }

    public interface ICreateSamplePathInfo
    {
        string PathName { get; set; } // 巡檢路線名稱
        int Frequency { get; set; } // 巡檢頻率
    }
    #endregion

    #region 巡檢路線模板-詳情
    public class SamplePathDetailViewModel : ISamplePath, IDefaultOrder
    {
        public string PathName { get; set; } // 巡檢路線名稱
        public int Frequency { get; set; } // 巡檢頻率
        public IEnumerable<IInspectionRFIDs> Equipments { get; set; }
    }
    #endregion

    #region 巡檢路線模板-編輯
    public class SamplePathEditViewModel : IEditSamplePath
    {
        [Required]
        [StringLength(9, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "巡檢路線編號")]
        public string PlanPathSN { get; set; } // 巡檢路線編號
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "巡檢路線名稱")]
        public string PathName { get; set; } // 巡檢路線名稱
        [Required]
        [Display(Name = "巡檢頻率")]
        public int Frequency { get; set; } // 巡檢頻率
        [Required]
        [Display(Name = "巡檢設備")]
        public IEnumerable<string> RFIDInternalCodes { get; set; } // 巡檢設備RFID清單
    }

    public interface IEditSamplePath : 
        IEditSamplePathInfo, IDefaultOrderModifiableList { }

    public interface IEditSamplePathInfo : ICreateSamplePathInfo
    {
        string PlanPathSN { get; set; } // 巡檢路線編號
    }
    #endregion

    #region 新增巡檢設備Grid
    /// <summary>
    /// 新增巡檢設備grid的查詢條件
    /// </summary>
    public class EquipmentRFIDSearchParamViewModel : GridParams, ISearchRFIDs, IInspectionRfidInfo
    {
        // 明確實作 RFIDName，使其在 ISearchRFIDs 及 SearchRFIDs 上不可見
        string IInspectionRfidInfo.RFIDName { get; set; }
        string IInspectionRfidInfo.RFIDMemo { get; set; }

        // Implements
        public string InternalCode { get; set; } // RFID內碼
        public string ExternalCode { get; set; } // RFID外碼
        public string EName { get; set; } // 設備名稱
        public string NO { get; set; } // 設備編號
        public string RFIDArea { get; set; } // RFID棟別(編號)
        public string RFIDFloor { get; set; } // RFID樓層(編號)
        public string Brand { get; set; } // 設備廠牌
        public string Model { get; set; } // 設備型號
        [Required]
        public int? Frequency { get; set; } // 巡檢頻率

        // Extras
        public List<string> InternalCodes { get; set; } // 已新增之RFID內碼
    }

    /// <summary>
    /// 新增巡檢設備grid
    /// </summary>
    public class InspectionRFIDsViewModel : IInspectionRFIDs
    {
        public string InternalCode { get; set; } // RFID內碼
        public string ExternalCode { get; set; } // RFID外碼
        public string RFIDName { get; set; } // RFID名稱
        public string RFIDArea { get; set; } // RFID棟別
        public string RFIDFloor { get; set; } // RFID樓層
        public string RFIDMemo { get; set; } // RFID備註
        public string EName { get; set; } // 設備名稱
        public string NO { get; set; } // 設備編號
        public string Brand { get; set; } // 設備廠牌
        public string Model { get; set; } // 設備型號
        public string Frequency { get; set; } // 巡檢頻率
        public string RFIDViewName { get; internal set; } // RFID樓層模型名稱(定位用)
        public decimal? Location_X { get; internal set; } // 座標X(定位用)
        public decimal? Location_Y { get; internal set; } // 座標Y(定位用)
    }

    public class RFIDQueryModel : InspectionRFIDsViewModel
    {
        public string ASN { get; set; } // RFID棟別編號
        public string FSN { get; set; } // RFID棟別編號
        public new int? Frequency { get; set; } // 巡檢頻率
    }

    /// <summary>
    /// 新增巡檢設備grid的查詢條件
    /// </summary>
    public interface ISearchRFIDs : IInspection, IInspectionRfidSearch, IInspectionEquipmentInfo { }

    public interface IInspection
    {
        int? Frequency { get; set; } // 巡檢頻率
    }

    public interface IInspectionRfidSearch
    {
        string InternalCode { get; set; } // RFID內碼
        string ExternalCode { get; set; } // RFID外碼
        string RFIDArea { get; set; } // RFID棟別
        string RFIDFloor { get; set; } // RFID樓層
    }
    #endregion

    //-----Interface
    public interface ISamplePath
    {
        string PathName { get; set; } // 巡檢路線名稱
        int Frequency { get; set; } // 巡檢頻率
    }

    public interface IDefaultOrder
    {
        IEnumerable<IInspectionRFIDs> Equipments { get; set; }
    }

    /// <summary>
    /// 巡檢RFID及設備資料
    /// </summary>
    public interface IInspectionRFIDs : IInspectionRfidInfo, IInspectionEquipmentInfo { }

    public interface IInspectionRfidInfo : IInspectionRfidSearch
    {
        string RFIDName { get; set; } // RFID名稱
        string RFIDMemo { get; set; } // RFID備註
    }

    public interface IInspectionEquipmentInfo
    {
        string EName { get; set; } // 設備名稱
        string NO { get; set; } // 設備編號
        string Brand { get; set; } // 設備廠牌
        string Model { get; set; } // 設備型號
    }

    

    public interface IDefaultOrderModifiableList
    {
        string PlanPathSN { get; set; } // 巡檢路線編號
        int Frequency { get; set; } // 巡檢頻率
        IEnumerable<string> RFIDInternalCodes { get; set; } // 巡檢設備RFID清單
    }

    #region Service使用
    public class DefaultOrderModifiableListInstance : IDefaultOrderModifiableList
    {
        public string PlanPathSN { get; set; } // 巡檢路線編號
        public int Frequency { get; set; } // 巡檢頻率
        public IEnumerable<string> RFIDInternalCodes { get; set; } // 巡檢設備RFID清單

        public DefaultOrderModifiableListInstance(string sn, SamplePathCreateViewModel data)
        {
            PlanPathSN = sn;
            Frequency = data.Frequency;
            RFIDInternalCodes = data.RFIDInternalCodes;
        }
    }
    #endregion
}