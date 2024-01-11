using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class SI_Info
    {
        [Required]
        [StringLength(1, ErrorMessage = "{0} 的長度最多1個字元。")]
        [Display(Name = "庫存種類")]
        public string StockType { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "庫存名稱")]
        public string StockName { get; set; }
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "廠商品名")]
        public string MName { get; set; }
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "尺寸")]
        public string Size { get; set; }
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "廠牌")]
        public string Brand { get; set; }
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "型號")]
        public string Model { get; set; }
        [Required]
        [Display(Name = "入庫數量")]
        public double Amount { get; set; }
        [Required]
        [StringLength(2, ErrorMessage = "{0} 的長度最多2個字元。")]
        [Display(Name = "單位")]
        public string Unit { get; set; }
        public DateTime ExpiryDate { get; set; }
        [Required]
        [StringLength(30, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多30個字元。", MinimumLength = 1)]
        [Display(Name = "擺放位置")]
        public string Location { get; set; }
        public double MinStockAmount { get; set; } = 0;
    }

    public class CheckInfo
    {
        [Required]
        [StringLength(1, ErrorMessage = "{0} 的長度最多1個字元。")]
        [Display(Name = "庫存種類")]
        public string StockType { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "庫存名稱")]
        public string StockName { get; set; }
        [Required]
        [StringLength(2, ErrorMessage = "{0} 的長度最多2個字元。")]
        [Display(Name = "單位")]
        public string Unit { get; set; }
    }
}