using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class Repair_ManagementWebCreateViewModel
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

    public class Repair_ManagementAddOrUpdateViewModel
    {
        public string RSN { get; set; }
        public string ESN { get; set; }
        public string ReportLevel { get; set; }
        public string ReportContent { get; set; }
        public HttpPostedFile ReportImg { get; set; }
        public string UserName { get; set; }
    }

    public class Repair_ManagementAppSearchEquipmentViewModel
    {
        public int? ASN { get; set; }
        public string FSN { get; set; }
        public string EName { get; set; }
        public string NO { get; set; }
    }

    public class Repair_ManagementRepairWorkSortViewModel
    {
        public string UserName { get; set; }
        public string OrderBy { get; set; }
        public string Order { get; set; }
    }

    public class Repair_ManagementRepairListFilterViewModel
    {
        public string UserName { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
    }
}