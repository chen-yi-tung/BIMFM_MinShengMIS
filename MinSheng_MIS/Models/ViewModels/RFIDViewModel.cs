using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace MinSheng_MIS.Models.ViewModels
{
    public class EquipRFIDDetail : CreateEquipRFID, IRFIDInfoDetail
    {
        public string ASN { get; set; } // 棟別編號
        public string AreaName { get; set; } // 棟別名稱
        public string FloorName { get; set; } // 樓層名稱
        public string RFIDViewName { get; set; } // RFID樓層模型名稱(定位用)

        // override
        public new string InternalCode
        {
            get => RFIDInternalCode;
            set => RFIDInternalCode = value;
        } // 將 InternalCode 映射到 RFIDInternalCode
        [JsonIgnore]
        public string RFIDInternalCode { get; set; } // 這裡儲存實際的 RFID 內碼

        // RFID外碼
        public new string ExternalCode
        {
            get => RFIDExternalCode;
            set => RFIDExternalCode = value;
        } // 將 InternalCode 映射到 RFIDInternalCode
        [JsonIgnore]
        public string RFIDExternalCode { get; set; } // 這裡儲存實際的 RFID 內碼
    }

    public interface IRFIDInfoDetail : IRFIDInfo
    {
        string ASN { get; set; } // 棟別編號
        string AreaName { get; set; } // 棟別名稱
        string FloorName { get; set; } // 樓層名稱
        string RFIDViewName { get; set; } // RFID樓層模型名稱(定位用)
    }

    public class CreateEquipRFID : IRFIDInfo
    {
        string IRFIDInfo.SARSN { get; set; }
        string IRFIDInfo.ESN { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "{0} 的長度最多{1}個字元。")]
        [Display(Name = "RFID內碼")]
        public string InternalCode { get; set; } // RFID內碼
        [Required]
        [Display(Name = "RFID外碼")]
        public string ExternalCode { get; set; } // RFID外碼
        [Required]
        [StringLength(5, ErrorMessage = "{0} 的長度最多5個字元。")]
        [Display(Name = "樓層編號")]
        public string FSN { get; set; } // 樓層編號
        [Required]
        public string Name { get; set; } // 名稱
        [Required]
        public Nullable<decimal> Location_X { get; set; } // X定位
        [Required]
        public Nullable<decimal> Location_Y { get; set; } // Y定位
        public string Memo { get; set; } // 備註
    }

    public class EditEquipRFID : CreateEquipRFID, IRFIDInfo
    {
        public string ESN { get; set; } // 設備資料(EquipmentInfo)編號
    }

    public interface IRFIDInfo
    {
        string InternalCode { get; set; } // RFID內碼
        string SARSN { get; set; } // 庫存異動紀錄編號
        string ESN { get; set; } // 設備資料(EquipmentInfo)編號
        string ExternalCode { get; set; } // RFID外碼
        string FSN { get; set; } // 樓層編號
        string Name { get; set; } // 名稱
        Nullable<decimal> Location_X { get; set; } // X定位
        Nullable<decimal> Location_Y { get; set; } // Y定位
        string Memo { get; set; } // 備註
    }

    /// <summary>
    /// 向RFIDService獲取Query使用
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
        public decimal? Location_X { get; set; } // 座標X(定位用)
        public decimal? Location_Y { get; set; } // 座標Y(定位用)
    }
}