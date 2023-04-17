using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;

namespace MinSheng_MIS.Models.ViewModels
{
    public class InspectionPlan_ManagementViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 巡檢計畫-紀錄 格式
        public class Root
        {
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanRecord InspectionPlanRecord { get; set; }
            public List<InspectionPlanMember> InspectionPlanMember { get; set; }
            public List<EquipmentMaintainRecord> EquipmentMaintainRecordList { get; set; }
            public List<EquipmentRepairRecord> EquipmentRepairRecordList { get; set; }
        }

        public class EquipmentMaintainRecord
        {
            public string IPMSN { get; set; }
            public string IPSN { get; set; }
            public string MaintainState { get; set; }
            public string Area { get; set; }
            public string Floor { get; set; }
            public string ESN { get; set; }
            public string EState { get; set; }
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
            public string PlanDate { get; set; }
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
            public string DateOfFilling { get; set; }
            public string InspectionRecord { get; set; }
            public List<string> ImgPath { get; set; }
        }
        #endregion

        #region 巡檢計畫-紀錄 DataGrid
        public string GetJsonForRecord(string IPSN)
        {
            try
            {
                #region 巡檢計畫資訊 
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
                MyNameString = string.Join("、", NameList);

                inspectionPlan.IPSN = IPSN;
                inspectionPlan.IPName = InsPlan.IPName;
                inspectionPlan.PlanDate = InsPlan.PlanDate.ToString("yyyy/MM/dd");
                var dic_IPS = Surfaces.Surface.InspectionPlanState();
                inspectionPlan.PlanState = dic_IPS[InsPlan.PlanState];
                var dic_Shift = Surfaces.Surface.Shift();
                inspectionPlan.Shift = dic_Shift[InsPlan.Shift];
                inspectionPlan.MyName = MyNameString;
                #endregion

                #region 巡檢完工填報
                InspectionPlanRecord inspectionPlanRecord = new InspectionPlanRecord();
                inspectionPlanRecord.PlanState = dic_IPS[InsPlan.PlanState];
                string InfoName = db.AspNetUsers.Where(x => x.UserName == InsPlan.InformatUserID).Select(x => x.MyName).FirstOrDefault();
                inspectionPlanRecord.MyName = InfoName;
                inspectionPlanRecord.DateOfFilling = InsPlan.DateOfFilling?.ToString("yyyy/MM/dd HH:mm:ss");
                inspectionPlanRecord.InspectionRecord = InsPlan.InspectionRecord;
                List<string> ImgList = new List<string>();
                var pathList = db.CompletionReportImage.Where(x => x.IPSN == IPSN).Select(x => x.ImgPath);
                foreach (var item in pathList)
                {
                    ImgList.Add(item);
                }
                inspectionPlanRecord.ImgPath = ImgList;
                #endregion

                #region 巡檢軌跡紀錄
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
                #endregion

                #region 設備保養紀錄
                List<EquipmentMaintainRecord> ListEMR = new List<EquipmentMaintainRecord>();
                var EMR = db.InspectionPlanMaintain.Where(x => x.IPSN == IPSN);
                foreach (var item in EMR)
                {
                    EquipmentMaintainRecord equipmentMaintainRecord = new EquipmentMaintainRecord();
                    equipmentMaintainRecord.IPMSN = item.IPMSN;
                    equipmentMaintainRecord.IPSN = IPSN;
                    var dic_IPMS = Surfaces.Surface.InspectionPlanMaintainState();
                    equipmentMaintainRecord.MaintainState = dic_IPMS[item.MaintainState];
                    var EMFI = db.EquipmentMaintainFormItem.Find(item.EMFISN);
                    var EMI = db.EquipmentMaintainItem.Find(EMFI.EMISN);
                    var EI = db.EquipmentInfo.Find(EMI.ESN);
                    var MI = db.MaintainItem.Find(EMI.MISN);
                    equipmentMaintainRecord.Area = EI.Area;
                    equipmentMaintainRecord.Floor = EI.Floor;
                    equipmentMaintainRecord.ESN = EI.ESN;
                    var dic_EState = Surfaces.Surface.EState();
                    equipmentMaintainRecord.EState = dic_EState[EI.EState];
                    equipmentMaintainRecord.EName = EI.EName;
                    equipmentMaintainRecord.EMFISN = item.EMFISN;
                    equipmentMaintainRecord.MIName = MI.MIName;
                    equipmentMaintainRecord.Unit = EMFI.Unit;
                    equipmentMaintainRecord.Period = EMFI.Period.ToString();
                    equipmentMaintainRecord.LastTime = EMFI.LastTime.ToString("yyyy/MM/dd");
                    equipmentMaintainRecord.NextTime = EMFI.NextTime?.ToString("yyyy/MM/dd");
                    ListEMR.Add(equipmentMaintainRecord);
                }
                #endregion

                #region 設備維修紀錄
                List<EquipmentRepairRecord> ListERR = new List<EquipmentRepairRecord>();
                var IPR = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN);
                foreach (var item in IPR)
                {
                    EquipmentRepairRecord equipmentRepairRecord = new EquipmentRepairRecord();
                    equipmentRepairRecord.IPRSN = item.IPRSN;
                    equipmentRepairRecord.IPSN = IPSN;
                    var dic_IPRS = Surfaces.Surface.InspectionPlanRepairState();
                    equipmentRepairRecord.RepairState = dic_IPRS[item.RepairState];
                    var ERF = db.EquipmentReportForm.Find(item.RSN);
                    var EI = db.EquipmentInfo.Find(ERF.ESN);
                    var dic_RL = Surfaces.Surface.ReportLevel();
                    equipmentRepairRecord.ReportLevel = dic_RL[ERF.ReportLevel];
                    equipmentRepairRecord.Area = EI.Area;
                    equipmentRepairRecord.Floor = EI.Floor;
                    equipmentRepairRecord.RSN = item.RSN;
                    equipmentRepairRecord.Date = ERF.Date.ToString("yyyy/MM/dd HH:mm:ss");
                    equipmentRepairRecord.ESN = ERF.ESN;
                    equipmentRepairRecord.EName = EI.EName;
                    var Name = db.AspNetUsers.Where(x => x.UserName == ERF.InformatUserID).Select(x => x.MyName).FirstOrDefault();
                    equipmentRepairRecord.InformantUserID = Name;
                    equipmentRepairRecord.ReportContent = ERF.ReportContent;
                    ListERR.Add(equipmentRepairRecord);
                }
                #endregion

                Root root = new Root();
                root.InspectionPlan = inspectionPlan;
                root.InspectionPlanRecord = inspectionPlanRecord;
                root.InspectionPlanMember = ListIP;
                root.EquipmentMaintainRecordList = ListEMR;
                root.EquipmentRepairRecordList = ListERR;

                string result = JsonConvert.SerializeObject(root);
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion
    }
}