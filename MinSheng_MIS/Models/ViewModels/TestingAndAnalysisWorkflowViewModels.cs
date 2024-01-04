using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class TA_Workflow
    {
        [Required]
        [StringLength(200, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多200個字元。", MinimumLength = 1)]
        [Display(Name = "實驗類型")]
        public string ExperimentType { get; set; } //實驗類型
        [Required]
        [StringLength(200, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多200個字元。", MinimumLength = 1)]
        [Display(Name = "實驗名稱")]
        public string ExperimentName { get; set; } //實驗名稱
        public List<string> LabelName { get; set; } //使用的實驗標籤名稱
        public List<string> DataName { get; set; } //實驗數據欄位名稱
        public HttpPostedFileBase WorkflowFile { get; set; } //新增的實驗採樣分析流程檔案
        //--------------------------------------------
        [Required]
        public string TAWSN { get; set; } //採驗分析流程編號
    }

    public class TA_Label
    {

    }

    public class TA_Data
    {

    }
}