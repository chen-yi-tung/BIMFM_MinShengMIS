using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class SR_Info
    {
        [Required]
        public string SRUserName { get; set; } //申請人
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "申請部門")]
        public string SRDept { get; set; } //申請部門
        [StringLength(200, ErrorMessage = "{0} 的長度最多200個字元。")]
        [Display(Name = "領用原因")]
        public string SRContent { get; set; } //領用原因
        [Required]
        public List<SR_Item> StoresRequisitionItem { get; set; } // 領用申請單項目
        //--------------------------------------------
        [Required]
        public string SRSN { get; set; } //領用申請單號
    }

    public class SR_Item
    {
        [Required]
        [StringLength(4, ErrorMessage = "{0} 非指定格式", MinimumLength = 1)]
        [Display(Name = "庫存項目編碼")]
        public string SISN { get; set; } //庫存項目編碼(品名下拉式選單value)
        [Required]
        [Display(Name = "數量")]
        public double Amount { get; set; } //數量(領用數量)
        [StringLength(200, ErrorMessage = "{0} 的長度最多200個字元。")]
        [Display(Name = "領用原因")]
        public string SRContent { get; set; } //領用原因
    }

    public class SR_Audit
    {
        [Required]
        public string SRSN { get; set; } //領用申請單號
        [StringLength(200, ErrorMessage = "{0} 的長度最多200個字元。")]
        [Display(Name = "審核意見")]
        public string AuditContent { get; set; } //審核意見
        [Required]
        public List<string> AuditResult { get; set; } // 審核結果
    }

    public class SR_ViewModel<T>
    {
        public string SRSN { get; set; }
        public string SRMyName { get; set; } //領用人員(選單名)
        public string SRUserName { get; set; } //申請領用人員(選單值)
        public string SRDept { get; set; }
        public string SRContent { get; set; }
        public List<T> StoresRequisitionItem { get; set; } = new List<T>();
    }

    public class Read_ItemViewModel
    {
        public string PickUpStatusName { get; set; }
        public string StockType { get; set; }
        public string StockName { get; set; }
        public double Amount { get; set; }
        public string Unit { get; set; }
        public string SRContent { get; set; }
    }

    public class Edit_ItemViewModel
    {
        public string SISN { get; set; }
        public string StockType { get; set; }
        public string StockName { get; set; }
        public double Amount { get; set; }
        public string Unit { get; set; }
        public string SRContent { get; set; }
    }

    public class Audit_ItemViewModel
    {
        public string SRISN { get; set; }
        public string StockType { get; set; }
        public string StockName { get; set; }
        public double Amount { get; set; }
        public double RemainingAmount { get; set; }
        public string Unit { get; set; }
        public string SRContent { get; set; }
    }
}