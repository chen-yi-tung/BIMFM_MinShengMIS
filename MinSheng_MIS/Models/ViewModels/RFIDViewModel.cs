using System.ComponentModel.DataAnnotations;

namespace MinSheng_MIS.Models.ViewModels
{
    public class EquipRFID : IRFIDInfo
    {
        [Required]
        [StringLength(24, ErrorMessage = "{0} 的長度最多24個字元。")]
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
        public decimal Location_X { get; set; } // X定位
        [Required]
        public decimal Location_Y { get; set; } // Y定位
        public string Memo { get; set; } // 備註
    }

    public interface IRFIDInfo
    {
        string InternalCode { get; set; } // RFID內碼
        string ExternalCode { get; set; } // RFID外碼
        string FSN { get; set; } // 樓層編號
        string Name { get; set; } // 名稱
        decimal Location_X { get; set; } // X定位
        decimal Location_Y { get; set; } // Y定位
        string Memo { get; set; } // 備註
    }
}