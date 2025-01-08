using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using MinSheng_MIS.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class SIRFID_ViewModel
    {
        [Required]
        [StringLength(24, ErrorMessage = "{0} 的長度最多24個字元。", MinimumLength = 1)]
        [Display(Name = "RFID內碼")]
        public string RFIDInternalCode { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。", MinimumLength = 1)]
        [Display(Name = "RFID外碼")]
        public string RFIDExternalCode { get; set; }

        //[Required]
        //[StringLength(1, ErrorMessage = "{0} 的長度最多1個字元。")]
        //[Display(Name = "庫存種類")] //Table StockType:StockTypeSN、StockTypeName
        //public int StockTypeSN { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，必填且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "庫存名稱")]//Table ComputationalStock:SISN
        public string StockName { get; set; }

        [FileSizeLimit(20)] // Limit file size to 20 MB
        public HttpPostedFileBase[] PurchaseOrder { get; set; } //採購單
    }

    public class SORFID_ViewModel
    {
        [Required]
        [RFIDInternalCodesValidation]
        public string[] RFIDInternalCodes { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元，且取用人必填。", MinimumLength = 1)]
        public string Recipient { get; set; }
    }

    public class SISNCount
    {
        public string SISN { get; set; }
        public int Count { get; set; }
    }
}