using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class SO_Info
    {
        [Required]
        [StringLength(10, ErrorMessage = "{0}格式錯誤。", MinimumLength = 1)]
        [Display(Name = "領用申請單號")]
        public string SRSN { get; set; }
        [Required]
        [Display(Name = "出庫人員")]
        public string StockOutUserName { get; set; }
        [Required]
        [Display(Name = "領取人員")]
        public string ReceiverUserName { get; set; }
        public string StockOutContent { get; set; }
        [Required]
        [Display(Name = "出庫項目")]
        public List<SO_Item> StockOutItem { get; set; }
    }

    public class SO_Item
    {
        [Required]
        [StringLength(10, ErrorMessage = "{0}格式錯誤。", MinimumLength = 1)]
        [Display(Name = "RFID")]
        public string SSN { get; set; }
        [Required]
        [StringLength(4, ErrorMessage = "{0}格式錯誤。", MinimumLength = 1)]
        [Display(Name = "庫存項目編號")]
        public string SISN { get; set; }
        [Required]
        public double OutAmount { get; set; }
    }

    public class SO_ViewModel
    {
        public string SORSN { get; set; }
        public string SRSN { get; set; }
        public string StockOutDateTime { get; set; }
        public string StockOutMyName { get; set; }
        public string ReceiverMyName { get; set; }
        public string StockOutContent { get; set; }
        public List<SO_Item_ViewModel> StockOutItem { get; set; }
    }

    public class SO_Item_ViewModel
    {
        public string SSN { get; set; }
        public string StockType { get; set; }
        public string StockName { get; set; }
        public string MName { get; set; }
        public string Size { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Unit { get; set; }
        public double OutAmount { get; set; }
    }

    public class SR_Item_ViewModel
    {
        public string PickUpStatus { get; set; }
        public string StockType { get; set; }
        public string StockName { get; set; }
        public double Amount { get; set; }
        public double TakeAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string Unit { get; set; }
        public string SISN { get; set; }
    }
}