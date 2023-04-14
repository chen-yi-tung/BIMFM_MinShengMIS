using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class InspectionPlan_ManagementViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        public class EquipmentMaintainRecord
        {
            public string IPMSN { get; set; }
            public string IPSN { get; set; }
            public string MaintainState { get; set; }
            public string Area { get; set; }
            public string Floor { get; set; }
            public string ESN { get; set; }
            public string EName { get; set; }
            public string EMFISN { get; set; }
            public string MIName { get; set; }
            public string Period { get; set; }
            public string Unit { get; set; }
            public string LastTime { get; set; }
            public string NextTime { get; set; }
        }

        public class EquipmentRepairRecord
        {
            public string IPRSN { get; set; }
            public string IPSN { get; set; }
            public string RepairState { get; set; }
            public string Area { get; set; }
            public string Floor { get; set; }
            public string ESN { get; set; }
            public string EName { get; set; }
            public string RSN { get; set; }
            public string ReportLevel { get; set; }
            public string Date { get; set; }
            public string InformantUserID { get; set; }
            public string ReportContent { get; set; }
        }

        public class InspectionPlan
        {
            public string IPSN { get; set; }
            public string IPName { get; set; }
            public DateTime PlanDate { get; set; }
            public string PlanState { get; set; }
            public string Shift { get; set; }
            public string MyName { get; set; }
        }

        public class InspectionPlanMember
        {
            public string MyName { get; set; }
            public string WatchID { get; set; }
            public string PMSN { get; set; }
        }

        public class InspectionPlanRecord
        {
            public string PlanState { get; set; }
            public string MyName { get; set; }
            public DateTime DateOfFilling { get; set; }
            public string InspectionRecord { get; set; }
            public List<string> ImgPath { get; set; }
        }

        public class Root
        {
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanRecord InspectionPlanRecord { get; set; }
            public List<InspectionPlanMember> InspectionPlanMember { get; set; }
            public EquipmentMaintainRecord EquipmentMaintainRecord { get; set; }
            public EquipmentRepairRecord EquipmentRepairRecord { get; set; }
        }


        public string GetJsonForRecord(string IPSN)
        {
            InspectionPlan inspectionPlan = new InspectionPlan();
            var InsPlan = db.InspectionPlan.Find(IPSN);
            var InsPlanMember = db.InspectionPlanMember.Where(x => x.IPSN == IPSN).Select(x => x.UserID);
            List<string> NameList = new List<string>();
            string MyNameString = string.Empty;
            foreach (var item in InsPlanMember)
            {
                var MyName = db.AspNetUsers.Where(x => x.UserName == item).Select(x => x.MyName).FirstOrDefault();
                NameList.Add(MyName);
            }
            MyNameString = string.Join(",", NameList);
            
            inspectionPlan.IPSN = IPSN;
            inspectionPlan.IPName = InsPlan.IPName;
            inspectionPlan.PlanDate = InsPlan.PlanDate;
            inspectionPlan.Shift = InsPlan.Shift;
            inspectionPlan.MyName = MyNameString;

            InspectionPlanRecord inspectionPlanRecord= new InspectionPlanRecord();
            inspectionPlanRecord.PlanState = InsPlan.PlanState;
            string InfoName = db.AspNetUsers.Where(x => x.UserName == InsPlan.InformatUserID).Select(x => x.MyName).FirstOrDefault();
            inspectionPlanRecord.MyName = InfoName;
            inspectionPlanRecord.DateOfFilling = (DateTime)InsPlan.DateOfFilling;
            inspectionPlanRecord.InspectionRecord = InsPlan.InspectionRecord;
            List<string> ImgList = new List<string>();
            var pathList = db.CompletionReportImage.Where(x => x.IPSN == IPSN).Select(x => x.ImgPath);
            foreach (var item in pathList)
            {
                ImgList.Add(item);
            }
            inspectionPlanRecord.ImgPath = ImgList;

            List<InspectionPlanMember> ListIP = new List<InspectionPlanMember>();
            var IPM = db.InspectionPlanMember.Where(x => x.IPSN == IPSN);
            foreach (var item in IPM) 
            {
                InspectionPlanMember inspectionPlanMember = new InspectionPlanMember();
                var ipmName = db.AspNetUsers.Where(x => x.UserName == item.UserID).Select(x => x.MyName).FirstOrDefault(); 
                inspectionPlanMember.MyName = ipmName;
                inspectionPlanMember.WatchID = item.WatchID;
                inspectionPlanMember.PMSN = item.PMSN;
                ListIP.Add(inspectionPlanMember);
            }


            EquipmentMaintainRecord equipmentMaintainRecord = new EquipmentMaintainRecord();
            EquipmentRepairRecord equipmentRepairRecord = new EquipmentRepairRecord();

            Root root = new Root();
            root.InspectionPlan = inspectionPlan;
            root.InspectionPlanRecord = inspectionPlanRecord;
            root.InspectionPlanMember = ListIP;
            root.EquipmentMaintainRecord= equipmentMaintainRecord;
            root.EquipmentRepairRecord= equipmentRepairRecord;

            string result = JsonConvert.SerializeObject(root);
            return result;
        }
    }
}