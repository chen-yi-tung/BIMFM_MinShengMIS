using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class EDRecord
    {
        [Required]
        public string TAWSN { get; set; } //採驗分析流程編號
        [Required]
        public DateTime EDate { get; set; } //實驗日期
        public HttpPostedFileBase EDFile { get; set; } //新增的實驗數據檔案
        public List<ED_Info> ExperimentalDataItem { get; set; } //實驗數據
        //--------------------------------------------
        [Required]
        public string EDRSN { get; set; } //實驗數據記錄編號
    }

    public class ED_Info
    {
        [Required]
        [StringLength(200, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多{1}個字元。", MinimumLength = 1)]
        [Display(Name = "數據欄位名稱")]
        public string DataName { get; set; }
        [Required]
        [Display(Name = "數據")]
        public string Data { get; set; }
    }

    public class ED_ViewModel
    {
        public string ExperimentType { get; set; } //實驗室維護單號
        public string ExperimentName { get; set; } //維護類型
        public string TAWSN { get; set; } //標題
        public string EDate { get; set; } //標題
        public string UploadUserName { get; set; } //上傳者
        public string UploadDateTime { get; set; } //上傳日期時間
        public string FilePath { get; set; } //維護檔案
        public List<ED_Info> ExperimentalDataItem { get; set; } = new List<ED_Info>();
    }
}