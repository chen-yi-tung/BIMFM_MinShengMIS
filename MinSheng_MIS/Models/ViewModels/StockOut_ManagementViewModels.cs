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
}