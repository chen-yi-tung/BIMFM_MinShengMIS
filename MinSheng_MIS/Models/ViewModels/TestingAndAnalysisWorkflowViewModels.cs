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
        [StringLength(200, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多{1}個字元。", MinimumLength = 1)]
        [Display(Name = "實驗類型")]
        public string ExperimentType { get; set; } //實驗類型
        [Required]
        [StringLength(200, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多{1}個字元。", MinimumLength = 1)]
        [Display(Name = "實驗名稱")]
        public string ExperimentName { get; set; } //實驗名稱
        public List<string> LabelName { get; set; } //使用的實驗標籤名稱
        public List<string> DataName { get; set; } //實驗數據欄位名稱
        public HttpPostedFileBase WorkflowFile { get; set; } //新增的實驗採樣分析流程檔案
        //--------------------------------------------
        [Required]
        public string TAWSN { get; set; } //採驗分析流程編號
    }

    public class TA_Workflow_ViewModel
    {
        public string TAWSN { get; set; } //實驗室維護單號
        public string ExperimentType { get; set; } //維護類型
        public string ExperimentName { get; set; } //標題
        public string UploadUserName { get; set; } //上傳者
        public string UploadDateTime { get; set; } //上傳日期時間
        public string FilePath { get; set; } //維護檔案
        public List<TA_Label> LabelNames { get; set; } = new List<TA_Label>();
        public List<TA_Data> DataNames { get; set; } = new List<TA_Data>();
    }

    public class TA_Label
    {
        public string LNSN { get; set; } //實驗標籤名稱編號
        public string LabelName { get; set; } //實驗標籤名稱
    }

    public class TA_Data
    {
        public string DNSN { get; set; } //實驗數據欄位名稱編號
        public string DataName { get; set; } //實驗數據欄位名稱
    }
}