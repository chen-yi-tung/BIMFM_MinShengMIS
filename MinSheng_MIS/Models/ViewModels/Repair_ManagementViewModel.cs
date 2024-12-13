using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class Repair_ManagementCreateViewModel
    {
        public string ESN { get; set; }
        public string ReportLevel { get; set; }
        public string ReportContent { get; set; }
        public HttpPostedFileBase ReportImg { get; set; }
    }

    public class Repair_ManagementAssignmentViewModel
    {
        public List<string> RSN { get; set; }
        public DateTime DueDate { get; set; }
        public List<string> RepairUserName { get; set; }
    }

    public class Repair_ManagementAuditViewModel
    {
        public string RSN { get; set; }
        public string AuditReason { get; set; }
        public bool AuditResult { get; set; }
    }
}