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
}