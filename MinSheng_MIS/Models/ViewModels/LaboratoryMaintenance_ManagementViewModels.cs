using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class LM_Info
    {
        [Required]
        [StringLength(200, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多200個字元。", MinimumLength = 1)]
        [Display(Name = "維護類型")]
        public string MType { get; set; } //維護類型
        [Required]
        [StringLength(200, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多200個字元。", MinimumLength = 1)]
        [Display(Name = "標題")]
        public string MTitle { get; set; } //標題
        [StringLength(200, ErrorMessage = "{0} 的長度最多200個字元。")]
        [Display(Name = "說明")]
        public string MContent { get; set; } //說明
        public HttpPostedFileBase MFile { get; set; } //新增的維護檔案
        //--------------------------------------------
        [Required]
        public string LMSN { get; set; } //請購單號
        public string AFileName { get; set; } //已刪除的相關文件
    }
}