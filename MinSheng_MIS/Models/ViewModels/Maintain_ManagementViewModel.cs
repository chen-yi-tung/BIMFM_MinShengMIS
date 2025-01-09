using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    // WEB
    #region 定期保養單 詳情
    public class Maintain_ManagementDetailViewModel
    {
        public string EMFSN { get; set; }
        public string Status { get; set; }
        public string MaintainName { get; set; }
        public string Maintainer { get; set; }
        public string ESN { get; set; }
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
    #endregion

    #region 定期保養單 派工
    public class Maintain_ManagementAssignmentViewModel
    {
        public List<string> EMFSN { get; set; }
        public DateTime NextMaintainDate { get; set; }
        public List<string> Maintainer { get; set; }
    }
    #endregion

    #region 定期保養單 審核
    public class Maintain_ManagementAuditViewModel
    {
        public string EMFSN { get; set; }
        public string AuditReason { get; set; }
        public bool AuditResult { get; set; }
    }
    #endregion

    // APP
    #region 定期保養單 列表
    public class MaintainManagementApp_ListViewModel
    {
        public string TotalNum {  get; set; }
        public string PendingNum { get; set; }
        public string NotApprovedNum { get; set; }
        public List<MaintainManagementApp_ListItem> MaintainFormList { get; set; }
        public class MaintainManagementApp_ListItem
        {
            public List<string> RFIDList{ get; set; }
            public string EMFSN { get; set; }
            public string Status { get; set; }
            public string EName { get; set; }
            public string NO { get; set; }
            public string EState { get; set; }
            public string Area { get; set; }
            public string FloorName { get; set; }
            public string DispatcherTime { get; set; }
            public string NextMaintainDate { get; set; }
        }
    }
    #endregion

    #region 定期保養單 填報詳情
    public class MaintainManagementApp_Detail
    {
        public string EMFSN { get; set; }
        public string EName { get; set; }
        public string NO { get; set; }
        public string Area { get; set; }
        public string FloorName { get; set; }
        public string ESN { get; set; }
        public string MaintainName { get; set; }
        public string Period { get; set; }
        public string LastMaintainDate { get; set; }
        
        // 審核內容
        public bool? IsAudited { get; set; }
        public string AuditReason { get; set; }
    }
    #endregion

    #region 定期保養單 填報
    public class MaintainManagementApp_Report
    {
        public string EMFSN { get; set; }
        public string ReportContent { get; set; }
    }
    #endregion

    #region 藍芽位置整理(借放)
    public class BluetoothJson
    {
        public string FSN { get; set; }
        public List<PIN> pins { get; set; }
        public class PIN
        {
            public string GUID { get; set; }
            public string DBID { get; set; }
            public string ElementID { get; set; }
            public string DeviceName { get; set; }
            public List<double> position { get; set; }
        }
    }
    #endregion
}