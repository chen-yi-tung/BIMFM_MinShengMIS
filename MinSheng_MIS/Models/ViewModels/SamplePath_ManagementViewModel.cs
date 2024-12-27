using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MinSheng_MIS.Models.ViewModels
{
    #region 巡檢路線模板-新增
    public class SamplePathCreateViewModel : ICreateSamplePath
    {
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
    #endregion

    #region 巡檢路線模板-詳情
    public class SamplePathDetailViewModel : ISamplePath, IDefaultOrder
    {
        public string PathName { get; set; } // 巡檢路線名稱
        public int Frequency { get; set; } // 巡檢頻率
        public IEnumerable<IInspectionRFIDs> Equipments { get; set; }
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
    }

    /// <summary>
    /// Service向RFIDService獲取Query使用
    /// </summary>
    public class RFIDServiceQueryModel : IInspectionRfidInfo, IInspectionRfidSearch
    {
        string IInspectionRfidSearch.RFIDArea { get; set; }
        string IInspectionRfidSearch.RFIDFloor { get; set; }

        // RFID內碼
        public string InternalCode 
        {
            get => RFIDInternalCode;
            set => RFIDInternalCode = value;
        } // 將 InternalCode 映射到 RFIDInternalCode
        public string RFIDInternalCode { get; set; } // 這裡儲存實際的 RFID 內碼

        // RFID外碼
        public string ExternalCode
        {
            get => RFIDExternalCode;
            set => RFIDExternalCode = value;
        } // 將 InternalCode 映射到 RFIDInternalCode
        public string RFIDExternalCode { get; set; } // 這裡儲存實際的 RFID 內碼

        // RFID名稱
        public string RFIDName
        {
            get => Name;
            set => Name = value;
        } // 將 RFIDName 映射到 Name
        public string Name { get; set; } // 這裡儲存實際的 RFID 名稱

        // RFID備註
        public string RFIDMemo
        {
            get => Memo;
            set => Memo = value;
        } // 將 RFIDName 映射到 Name
        public string Memo { get; set; } // 這裡儲存實際的 RFID 備註

        public Floor_Info Floor_Info { get; set; } // RFID樓層
        public EquipmentInfo EquipmentInfo { get; set; } // RFID樓層
    }

    public class RFIDQueryModel : InspectionRFIDsViewModel
    {
        public string ASN { get; set; } // RFID棟別編號
        public string FSN { get; set; } // RFID棟別編號
        public new int? Frequency { get; set; } // 巡檢頻率
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

    public interface ICreateSamplePath : ISamplePathModifiableList
    {
        IEnumerable<string> RFIDInternalCodes { get; set; } // 巡檢設備RFID清單
    }

    public interface ISamplePathModifiableList
    {
        string PathName { get; set; } // 巡檢路線名稱
        int Frequency { get; set; } // 巡檢頻率
    }

    public interface IDefaultOrderModifiableList
    {
        string PlanPathSN { get; set; } // 巡檢路線編號
        IEnumerable<string> RFIDInternalCodes { get; set; } // 巡檢設備RFID清單
    }

    /// <summary>
    /// 新增巡檢設備grid的查詢條件
    /// </summary>
    public interface ISearchRFIDs : IInspection, IInspectionRfidSearch, IInspectionEquipmentInfo { }

    /// <summary>
    /// 巡檢RFID及設備資料
    /// </summary>
    public interface IInspectionRFIDs : IInspectionRfidInfo, IInspectionEquipmentInfo { }

    public interface IInspectionEquipmentInfo
    {
        string EName { get; set; } // 設備名稱
        string NO { get; set; } // 設備編號
        string Brand { get; set; } // 設備廠牌
        string Model { get; set; } // 設備型號
    }

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

    public interface IInspectionRfidInfo : IInspectionRfidSearch
    {
        string RFIDName { get; set; } // RFID名稱
        string RFIDMemo { get; set; } // RFID備註
    }

    #region Service使用
    public class DefaultOrderModifiableListInstance : IDefaultOrderModifiableList
    {
        public string PlanPathSN { get; set; } // 巡檢路線編號
        public IEnumerable<string> RFIDInternalCodes { get; set; } // 巡檢設備RFID清單

        public DefaultOrderModifiableListInstance(string sn, SamplePathCreateViewModel data)
        {
            PlanPathSN = sn;
            RFIDInternalCodes = data.RFIDInternalCodes;
        }
    }
    #endregion
}