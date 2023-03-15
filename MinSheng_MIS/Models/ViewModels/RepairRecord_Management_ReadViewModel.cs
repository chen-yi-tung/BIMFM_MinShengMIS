using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class RepairRecord_Management_ReadViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        private class EquipmentReportItem
        {
            public string ReportState { get; set; }
            public string RSN { get; set; }
            public string Date { get; set; }
            public string ReportLevel { get; set; }
            public string MyName { get; set; }
            public string Area { get; set; }
            public string Floor { get; set; }
            public string PorpertyCode { get; set; }
            public string ESN { get; set; }
            public string EName { get; set; }
            public string ESN_Button { get; set; }
            public string ReportContent { get; set; }
            public List<string> ImgPath { get; set; }
        }

        private class InspectionPlan
        {
            public string IPSN { get; set; }
            public string IPName { get; set; }
            public string PlanDate { get; set; }
            public string PlanState { get; set; }
            public string Shift { get; set; }
            public string MyName { get; set; }
        }

        private class InspectionPlanList
        {
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanRepair InspectionPlanRepair { get; set; }
            public List<RepairSupplementaryInfo> RepairSupplementaryInfo { get; set; }
            public List<RepairAuditInfo> RepairAuditInfo { get; set; }
        }

        private class InspectionPlanRepair
        {
            public string RepairState { get; set; }
            public string MyName { get; set; }
            public string RepairContent { get; set; }
            public string RepairDate { get; set; }
            public List<string> ImgPath { get; set; }
        }

        private class RepairAuditInfo
        {
            public string MyName { get; set; }
            public string AuditDate { get; set; }
            public string AuditResult { get; set; }
            public string AuditMemo { get; set; }
            public List<string> ImgPath { get; set; }
        }

        private class RepairSupplementaryInfo
        {
            public string MyName { get; set; }
            public string SupplementaryDate { get; set; }
            public string SupplementaryContent { get; set; }
            public List<string> FilePath { get; set; }
        }

        private class Root
        {
            public EquipmentReportItem EquipmentReportItem { get; set; }
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanRepair InspectionPlanRepair { get; set; }
            public List<RepairSupplementaryInfo> RepairSupplementaryInfo { get; set; }
            public List<RepairAuditInfo> RepairAuditInfo { get; set; }
            public List<InspectionPlanList> InspectionPlanList { get; set; }
        }
        public string GetJsonForRead(string RSN) {

            Root root = new Root();

            #region 處理報修資料 EquipmentReportItem
            EquipmentReportItem equipmentReportItem = new EquipmentReportItem();
            var table1 = db.EquipmentReportForm.Where(x=>x.RSN == RSN).ToList();
            foreach (var item in table1)
            {
                var ReportStatedics = Surface.EquipmentReportFormState();
                equipmentReportItem.ReportState = ReportStatedics[item.ReportState.Trim()];
                equipmentReportItem.RSN= item.RSN.Trim();
                equipmentReportItem.Date = item.Date?.ToString("yyyy/M/d HH:mm:ss");

                var ReportLeveldics = Surface.ReportLevel();
                equipmentReportItem.ReportLevel= ReportLeveldics[item.ReportLevel.Trim()];

                var myname = db.AspNetUsers.Where(x=>x.UserName == item.InformatUserID).Select(x=>x.MyName).FirstOrDefault();
                equipmentReportItem.MyName = myname;
                
                var EquipInfoRow = db.EquipmentInfo.Where(x => x.ESN == item.ESN).FirstOrDefault();
                equipmentReportItem.Area = EquipInfoRow.Area;
                equipmentReportItem.Floor= EquipInfoRow.Floor;
                equipmentReportItem.PorpertyCode = EquipInfoRow.PropertyCode;
                
                equipmentReportItem.ESN = item.ESN;
                equipmentReportItem.EName = EquipInfoRow.EName;
                equipmentReportItem.ReportContent = item.ReportContent;
                var ImagePathList = db.ReportImage.Where(x => x.RSN == item.RSN).Select(x=>x.ImgPath).ToList();
                equipmentReportItem.ImgPath = ImagePathList;
                break;
            }
            #endregion

            
            
           
            var IPSNlist = db.InspectionPlanRepair.Where(x => x.RSN == RSN).OrderBy(x => x.IPSN).Select(x => x.IPSN).ToList();
            foreach (var IPSN in IPSNlist)
            {
                #region 計劃資訊InspectionPlan
                InspectionPlanList inspectionPlan = new InspectionPlanList();
                var inspectionplantable = db.InspectionPlan.Find(IPSN);
                var IP = new InspectionPlan();
                IP.IPSN = inspectionplantable.IPSN;
                IP.IPName = inspectionplantable.IPName;
                IP.PlanDate = inspectionplantable.PlanDate.ToString("yyyy/M/d");
                var PSdics = Surface.InspectionPlanState();
                IP.PlanState = PSdics[inspectionplantable.PlanState];
                var Shiftdics = Surface.Shift();
                IP.Shift = Shiftdics[inspectionplantable.Shift];
                var IPUserudlist = db.InspectionPlanMember.Where(x => x.IPSN == IPSN).Select(x => x.UserID).ToList();
                var INSPNameList = "";
                int a = 0;
                foreach (var id in IPUserudlist)
                {
                    var myname = db.AspNetUsers.Where(x => x.UserName == id).Select(x => x.MyName).FirstOrDefault();
                    if (myname != null)
                    {
                        if (a == 0)
                            INSPNameList += myname;
                        else
                            INSPNameList += $"、{myname}";
                    }
                    a++;
                }
                a = 0;
                IP.MyName = INSPNameList;

                inspectionPlan.InspectionPlan = IP;
                #endregion

                #region 維修資料
                var inspectionPR = new InspectionPlanRepair();
                var inspectionPlanRepairtable = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN && x.RSN == RSN).FirstOrDefault();

                var IPRdics = Surface.InspectionPlanRepairState();
                inspectionPR.RepairState = IPRdics[inspectionPlanRepairtable.RepairState];
                inspectionPR.RepairContent = inspectionPlanRepairtable.RepairContent;

                var IPRname = db.AspNetUsers.Where(x => x.UserName == inspectionPlanRepairtable.RepairUserID).Select(x => x.MyName).FirstOrDefault() as string;
                inspectionPR.MyName = IPRname;
                inspectionPR.RepairDate = inspectionPlanRepairtable.RepairDate?.ToString("yyyy/M/d");

                var RPImgPathlist = db.RepairCompletionImage.Where(x => x.IPRSN == inspectionPlanRepairtable.IPRSN).Select(x => x.ImgPath).ToList();
                inspectionPR.ImgPath = RPImgPathlist;

                inspectionPlan.InspectionPlanRepair = inspectionPR;
                #endregion
            }
            

            InspectionPlanRepair inspectionPlanRepair = new InspectionPlanRepair();


            var  repairSupplementaryInfolist = new List<RepairSupplementaryInfo>();


            var RepairAuditInfolist = new List<RepairAuditInfo>();


            var InspectionPlanList = new List<InspectionPlanList>();




            root.EquipmentReportItem= equipmentReportItem;
            //root.InspectionPlan = inspectionplan;
            root.InspectionPlanRepair = inspectionPlanRepair;
            root.RepairSupplementaryInfo= repairSupplementaryInfolist;
            root.RepairAuditInfo = RepairAuditInfolist;
            root.InspectionPlanList= InspectionPlanList;

            string result = JsonConvert.SerializeObject(root);
            return result;
        }
    }
}