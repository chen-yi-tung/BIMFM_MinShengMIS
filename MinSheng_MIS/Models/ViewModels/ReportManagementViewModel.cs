using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class ReportManagementViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        public class InspectionPlanList
        {
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanRepair InspectionPlanRepair { get; set; }
            public List<RepairSupplementaryInfo> RepairSupplementaryInfo { get; set; }
            public List<RepairAuditInfo> RepairAuditInfo { get; set; }
            public string IPSN { get; set; }
            public string IPName { get; set; }
            public string PlanDate { get; set; }
            public string PlanState { get; set; }
            public string Shift { get; set; }
            public string MyName { get; set; }
        }

        public class InspectionPlanRepair
        {
            public string RepairState { get; set; }
            public string RepairContent { get; set; }
            public string MyName { get; set; }
            public string RepairDate { get; set; }
            public List<string> ImgPath { get; set; }
        }

        public class RepairAuditInfo
        {
            public string MyName { get; set; }
            public string AuditDate { get; set; }
            public string AuditResult { get; set; }
            public string AuditMemo { get; set; }
            public List<string> ImgPath { get; set; }
        }

        public class RepairSupplementaryInfo
        {
            public string MyName { get; set; }
            public string SupplementaryDate { get; set; }
            public string SupplementaryContent { get; set; }
            public List<string> FilePath { get; set; }
        }

        public class Root
        {
            public string RSN { get; set; }
            public string Date { get; set; }
            public string ReportLevel { get; set; }
            public string MyName { get; set; }
            public string ReportState { get; set; }
            public string Area { get; set; }
            public string Floor { get; set; }
            public string PropertyCode { get; set; }
            public string ESN { get; set; }
            public string EName { get; set; }
            public string ReportContent { get; set; }
            public List<string> ImgPath { get; set; }
            public List<InspectionPlanList> InspectionPlanList { get; set; }
        }
        public string GetJsonForRead(string RSN)
        {
            Root root = new Root();
            //var SourceTable = from x1 in db.EquipmentReportForm //主表
            //                  join x2 in db.AspNetUsers on x1.InformatUserID equals x2.UserName//找尋使用者名稱
            //                  join x3 in db.EquipmentInfo on x1.ESN equals x3.ESN
            //                  join x4 in db.ReportImage on x1.RSN equals x4.RSN
            //                  select new { x1.ReportState, x1.ReportLevel, x2.Area, x2.Floor, x1.ReportSource, x1.RSN, x1.Date, x2.PropertyCode, x1.ESN, x2.EName, x1.ReportContent, x3.MyName, x3.UserName };


            string result = JsonConvert.SerializeObject(root);
            return result;
        }
    }
}