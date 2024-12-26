using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class Maintain_ManagementDetailViewModel
    {
        public string EMFSN { get; set; }
        public string Status { get; set; }
        public string MaintainName { get; set; }
        public string Maintainer { get; set; }
        public string EName { get; set; }
        public string EState { get; set; }
        public string NO { get; set; }
        public string Location { get; set; }
        public string Period { get; set; }
        public string LastMaintainDate { get; set; }
        public string NextMaintainDate { get; set; }
        public string ReportId { get; set; }
        public string ReportTime { get; set; }
        public string ReportContent { get; set; }
        public string AuditResult { get; set; }
        public string AuditId { get; set; }
        public string AuditTime { get; set; }
        public string AuditReason { get; set; }
    }

    public class Maintain_ManagementAssignmentViewModel
    {
        public List<string> EMFSN { get; set; }
        public DateTime NextMaintainDate { get; set; }
        public List<string> Maintainer { get; set; }
    }
}