using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class EL_Info
    {
        [Required]
        public string TAWSN { get; set; } //採驗分析流程編號
        [Required]
        public DateTime EDate { get; set; } //實驗日期
        public List<string> LabelName { get; set; } //實驗標籤名稱
        //--------------------------------------------
        [Required]
        public string ELSN { get; set; } //實驗標籤編號
    }

    public class EL_ViewModel
    {
        public string ExperimentType { get; set; } //實驗室維護單號
        public string ExperimentName { get; set; } //維護類型
        public string TAWSN { get; set; } //標題
        public string EDate { get; set; } //標題
        public string UploadUserName { get; set; } //上傳者
        public string UploadDateTime { get; set; } //上傳日期時間
        public List<EL_Item> LaboratoryLabelItem { get; set; } = new List<EL_Item>();
    }

    public class EL_Item
    {
        public string ELISN { get; set; }
        public string LabelName { get; set; }
    }
}