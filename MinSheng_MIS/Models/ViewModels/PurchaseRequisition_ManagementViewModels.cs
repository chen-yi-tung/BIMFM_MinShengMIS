using MinSheng_MIS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class PR_Info
    {
        [Required]
        public string PRUserName { get; set; } //請購人
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "請購部門")]
        public string PRDept { get; set; } //請購部門
        [Required]
        public List<PR_Item> PurchaseRequisitionItem { get; set; } // 請購單項目
        //--------------------------------------------
        [Required]
        public string PRN { get; set; } //請購單號
        [Required]
        public string PRState { get; set; } //請購單狀態
        public DateTime? AuditDate { get; set; } //審核日期
        public string AuditResult { get; set; } //審核結果說明
        public HttpPostedFileBase AFile { get; set; } //新增的相關文件
        public string AFileName { get; set; } //已刪除的相關文件
    }

    public class PR_Item
    {
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "類別")]
        public string Kind { get; set; } //類別(選單值)
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "品名")]
        public string ItemName { get; set; } //品名
        [StringLength(50, ErrorMessage = "{0} 的長度最多50個字元。")]
        [Display(Name = "尺寸")]
        public string Size { get; set; } //尺寸
        [Required]
        [Display(Name = "數量")]
        public double PRAmount { get; set; } //數量(請購量)
        [Required]
        [StringLength(10, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多10個字元。", MinimumLength = 1)]
        [Display(Name = "單位")]
        public string Unit { get; set; } //單位(選單值)
        [StringLength(200, ErrorMessage = "{0} 的長度最多200個字元。")]
        [Display(Name = "申請用途")]
        public string ApplicationPurpose { get; set; } //申請用途
        //----------------資料顯示使用-------------------
        public string KindName { get; set; } //類別(選單名)
        public string UnitName { get; set; } //單位(選單名)
    }

    public class PR_ViewModel
    {
        public string PRN { get; set; } //請購單號
        public string PRUserName { get; set; } //請購人(選單值)
        public string PRUserAccount { get; set; } //請購人(選單名)
        public string PRState { get; set; } //請購單狀態(選單值)
        public string PRStateName { get; set; } //請購單狀態(選單名)
        public string PRDept { get; set; } //請購部門
        public string PRDate { get; set; } //請購申請日
        public string AuditDate { get; set; } //審核日期
        public string AuditResult { get; set; } //審核結果說明
        public string FilePath { get; set; } //相關文件
        public List<PR_Item> PurchaseRequisitionItem { get; set; } // 請購單項目
    }
}

