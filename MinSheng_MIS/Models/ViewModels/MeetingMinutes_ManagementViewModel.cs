using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace MinSheng_MIS.Models.ViewModels
{
    public class MeetingMinutesInfo
    {
        public string MMSN { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多200個字元。", MinimumLength = 1)]
        [Display(Name = "會議主題")]
        public string MeetingTopic { get; set; } //會議主題
        [Required]
        [StringLength(200, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多200個字元。", MinimumLength = 1)]
        [Display(Name = "會議地點")]
        public string MeetingVenue { get; set; } //會議地點
        [Required]
        [Display(Name = "會議日期")]
        public DateTime MeetingDate { get; set; } //會議日期
        [Required]
        [Display(Name = "會議時間(起)")]
        public DateTime MeetingDateStart { get; set; } //會議時間(起)
        [Required]
        [Display(Name = "會議時間(迄)")]
        public DateTime MeetingDateEnd { get; set; } //會議時間(迄)
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "會議主席")]
        public string Chairperson { get; set; } //會議主席
        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 1)]
        [Display(Name = "會議紀錄負責人")]
        public string TakeTheMinutes { get; set; } //會議紀錄負責人
        [Required]
        [Display(Name = "應到")]
        public int ExpectedAttendence { get; set; } //應到
        [Required]
        [Display(Name = "實到")]
        public int ActualAttendence { get; set; } //實到
        [Required]
        [Display(Name = "未到")]
        public int AbsenteeList { get; set; } //未到
        [Required]
        [Display(Name = "參與者")]
        public string Participant { get; set; } //參與者
        [StringLength(256, ErrorMessage = "{0} 的長度至少必須為{2}個字元，且最多50個字元。", MinimumLength = 0)]
        [Display(Name = "議題順序")]
        public string Agenda { get; set; } //議題順序
        [Display(Name = "會議內容")]
        public string MeetingContent { get; set; } //會議內容
        public HttpPostedFileBase MeetingFile { get; set; } //會議記錄文件
        public string MeetingFileName { get; set; }
    }
}