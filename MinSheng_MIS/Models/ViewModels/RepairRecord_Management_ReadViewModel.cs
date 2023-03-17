using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Xml.Linq;

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

        public string GetJsonForRead(string IPRSN) //巡檢維修紀錄-詳情
        {

            Root root = new Root();

            #region 處理報修資料 EquipmentReportItem

            EquipmentReportItem equipmentReportItem = new EquipmentReportItem();

            var InsepecPlanRe = db.InspectionPlanRepair.Where(x => x.IPRSN == IPRSN).FirstOrDefault();
            var table1 = db.EquipmentReportForm.Where(x => x.RSN == InsepecPlanRe.RSN).ToList();
            foreach (var item in table1)
            {
                var ReportStatedics = Surface.EquipmentReportFormState();
                equipmentReportItem.ReportState = ReportStatedics[item.ReportState.Trim()];
                equipmentReportItem.RSN = item.RSN.Trim();
                equipmentReportItem.Date = item.Date?.ToString("yyyy/M/d HH:mm:ss");

                var ReportLeveldics = Surface.ReportLevel();
                equipmentReportItem.ReportLevel = ReportLeveldics[item.ReportLevel.Trim()];

                var myname = db.AspNetUsers.Where(x => x.UserName == item.InformatUserID).Select(x => x.MyName).FirstOrDefault();
                equipmentReportItem.MyName = myname;

                var EquipInfoRow = db.EquipmentInfo.Where(x => x.ESN == item.ESN).FirstOrDefault();
                equipmentReportItem.Area = EquipInfoRow.Area;
                equipmentReportItem.Floor = EquipInfoRow.Floor;
                equipmentReportItem.PorpertyCode = EquipInfoRow.PropertyCode;

                equipmentReportItem.ESN = item.ESN;
                equipmentReportItem.EName = EquipInfoRow.EName;
                equipmentReportItem.ReportContent = item.ReportContent;
                var ImagePathList = db.ReportImage.Where(x => x.RSN == item.RSN).Select(x => x.ImgPath).ToList();
                equipmentReportItem.ImgPath = ImagePathList;
                break;
            }
            #endregion

            #region 計劃資訊InspectionPlan

            InspectionPlan inspectionPlan = new InspectionPlan();

            var InspecPlan = db.InspectionPlan.Where(x => x.IPSN == InsepecPlanRe.IPSN).FirstOrDefault();

            inspectionPlan.IPSN = InspecPlan.IPSN;
            inspectionPlan.IPName = InspecPlan.IPName;
            inspectionPlan.PlanDate = InspecPlan.PlanDate.ToString("yyyy/M/d");
            var PSdics = Surface.InspectionPlanState();
            inspectionPlan.PlanState = PSdics[InspecPlan.PlanState];
            var Shiftdics = Surface.Shift();
            inspectionPlan.Shift = Shiftdics[InspecPlan.Shift];
            var IPUserudlist = db.InspectionPlanMember.Where(x => x.IPSN == InspecPlan.IPSN).Select(x => x.UserID).ToList();
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
            inspectionPlan.MyName = INSPNameList;
            #endregion

            #region 維修資料

            InspectionPlanRepair inspectionPlanRepair = new InspectionPlanRepair();

            var IPRdics = Surface.InspectionPlanRepairState();
            inspectionPlanRepair.RepairState = IPRdics[InsepecPlanRe.RepairState];
            inspectionPlanRepair.RepairContent = InsepecPlanRe.RepairContent;

            var IPRname = db.AspNetUsers.Where(x => x.UserName == InsepecPlanRe.RepairUserID).Select(x => x.MyName).FirstOrDefault() as string;
            inspectionPlanRepair.MyName = IPRname;
            inspectionPlanRepair.RepairDate = InsepecPlanRe.RepairDate?.ToString("yyyy/M/d");

            var RPImgPathlist = db.RepairCompletionImage.Where(x => x.IPRSN == InsepecPlanRe.IPRSN).Select(x => x.ImgPath).ToList();
            inspectionPlanRepair.ImgPath = RPImgPathlist;
            #endregion

            #region RepairSupplementaryInfo補件資料

            var repairSupplementaryInfolist = new List<RepairSupplementaryInfo>(); //補件資料List

            var RSInfoData = db.RepairSupplementaryInfo.Where(x => x.IPRSN == IPRSN).ToList();
            foreach (var Data in RSInfoData)
            {
                RepairSupplementaryInfo RSInfo = new RepairSupplementaryInfo(); //補件資料

                var Name = db.AspNetUsers.Where(x => x.UserName == Data.SupplementaryUserID).FirstOrDefault();
                RSInfo.MyName = Name.MyName;
                RSInfo.SupplementaryDate = Data.SupplementaryDate.ToString("yyyy/M/d");
                RSInfo.SupplementaryContent = Data.SupplementaryContent;
                var FileP = db.RepairSupplementaryFile.Where(x => x.PRSN == Data.PRSN).Select(x => x.FilePath).ToList();
                RSInfo.FilePath = FileP;
                repairSupplementaryInfolist.Add(RSInfo);
            }
            #endregion

            #region RepairAuditInfo審核資料

            var RepairAuditInfolist = new List<RepairAuditInfo>(); //審核資料List

            var RepairAuIn = db.RepairAuditInfo.Where(x => x.IPRSN == IPRSN).Where(x => x.IsBuffer == false).ToList();
            foreach (var RAI in RepairAuIn)
            {
                RepairAuditInfo RAInfo = new RepairAuditInfo(); //審核資料

                var AName = db.AspNetUsers.Where(x => x.UserName == RAI.AuditUserID).Select(x => x.MyName).FirstOrDefault();
                RAInfo.MyName = AName;
                RAInfo.AuditDate = RAI.AuditDate.ToString("yyyy/M/d");
                RAInfo.AuditResult = RAI.AuditResult;
                RAInfo.AuditMemo = RAI.AuditMemo;
                var ImgP = db.RepairAuditImage.Where(x => x.PRASN == RAI.PRASN).Select(x => x.ImgPath).ToList();
                RAInfo.ImgPath = ImgP;
                RepairAuditInfolist.Add(RAInfo);
            }
            #endregion

            #region InspectionPlanList報修單相關維修紀錄
            var InspectionPlanList = new List<InspectionPlanList>();

            var restRSNinInspPlanRe = db.InspectionPlanRepair.Where(x => x.RSN.Contains(InsepecPlanRe.RSN)).Where(x => x.IPRSN != IPRSN).ToList();
            foreach (var item in restRSNinInspPlanRe)
            {
                InspectionPlanList inspectionPlans = new InspectionPlanList();

                InspectionPlan inspection = new InspectionPlan(); //計劃資訊
                var InsPlan = db.InspectionPlan.Where(x => x.IPSN == item.IPSN).FirstOrDefault();
                inspection.IPSN = InsPlan.IPSN;
                inspection.IPName = InsPlan.IPName;
                inspection.PlanDate = InsPlan.PlanDate.ToString("yyyy/M/d");

                inspection.PlanState = PSdics[InsPlan.PlanState];
                inspection.Shift = Shiftdics[InsPlan.Shift];
                var UID = db.InspectionPlanMember.Where(x => x.IPSN == InsPlan.IPSN).Select(x => x.UserID).ToList();
                var INSPNlist = "";
                int aa = 0;
                foreach (var id in UID)
                {
                    var myname1 = db.AspNetUsers.Where(x => x.UserName == id).Select(x => x.MyName).FirstOrDefault();
                    if (myname1 != null)
                    {
                        if (aa == 0)
                            INSPNlist += myname1;
                        else
                            INSPNlist += $"、{myname1}";
                    }
                    aa++;
                }
                aa = 0;
                inspection.MyName = INSPNlist;
                inspectionPlans.InspectionPlan = inspection;




                InspectionPlanRepair planRepair = new InspectionPlanRepair(); //維修資訊
                planRepair.RepairState = item.RepairState;
                planRepair.RepairContent = item.RepairContent;

                var Name2 = db.AspNetUsers.Where(x => x.UserName == item.RepairUserID).Select(x => x.MyName).FirstOrDefault();
                planRepair.MyName = Name2;
                planRepair.RepairDate = item.RepairDate?.ToString("yyyy/M/d HH:mm:ss");

                var RPImg2 = db.RepairCompletionImage.Where(x => x.IPRSN == item.IPRSN).Select(x => x.ImgPath).ToList();
                planRepair.ImgPath = RPImg2;
                inspectionPlans.InspectionPlanRepair = planRepair;





                List<RepairSupplementaryInfo> ListRSI = new List<RepairSupplementaryInfo>(); //補件資料
                var RSInfoData2 = db.RepairSupplementaryInfo.Where(x => x.IPRSN == IPRSN).ToList();
                foreach (var item2 in RSInfoData2)
                {
                    RepairSupplementaryInfo RSI = new RepairSupplementaryInfo();

                    var Name3 = db.AspNetUsers.Where(x => x.UserName == item2.SupplementaryUserID).FirstOrDefault();
                    RSI.MyName = Name3.MyName;
                    RSI.SupplementaryDate = item2.SupplementaryDate.ToString("yyyy/M/d");
                    RSI.SupplementaryContent = item2.SupplementaryContent;
                    var FileP2 = db.RepairSupplementaryFile.Where(x => x.PRSN == item2.PRSN).Select(x => x.FilePath).ToList();
                    RSI.FilePath = FileP2;
                    ListRSI.Add(RSI);
                }
                inspectionPlans.RepairSupplementaryInfo = ListRSI;




                List<RepairAuditInfo> ListRAI = new List<RepairAuditInfo>(); //審核資料

                var RepairAuIn2 = db.RepairAuditInfo.Where(x => x.IPRSN == item.IPRSN).Where(x => x.IsBuffer == false).ToList();
                foreach (var item3 in RepairAuIn2)
                {
                    RepairAuditInfo RAI = new RepairAuditInfo(); //審核資料

                    var AName2 = db.AspNetUsers.Where(x => x.UserName == item3.AuditUserID).Select(x => x.MyName).FirstOrDefault();
                    RAI.MyName = AName2;
                    RAI.AuditDate = item3.AuditDate.ToString("yyyy/M/d");
                    RAI.AuditResult = item3.AuditResult;
                    RAI.AuditMemo = item3.AuditMemo;
                    var ImgP2 = db.RepairAuditImage.Where(x => x.PRASN == item3.PRASN).Select(x => x.ImgPath).ToList();
                    RAI.ImgPath = ImgP2;
                    ListRAI.Add(RAI);
                }
                inspectionPlans.RepairAuditInfo = ListRAI;


                InspectionPlanList.Add(inspectionPlans);
            }
            #endregion

            root.EquipmentReportItem = equipmentReportItem;
            root.InspectionPlan = inspectionPlan;
            root.InspectionPlanRepair = inspectionPlanRepair;
            root.RepairSupplementaryInfo = repairSupplementaryInfolist;
            root.RepairAuditInfo = RepairAuditInfolist;
            root.InspectionPlanList = InspectionPlanList;

            string result = JsonConvert.SerializeObject(root);
            return result;
        }

        public class AllRepairAudit
        {
            public string AuditUserID { get; set; }
            public string AuditMemo { get; set; }
            public string ImgPath { get; set; }
            public string AuditResult { get; set; }
            public string PRASN { get; set; }
            public string IPRSN { get; set; }
            public string AuditDate { get; set; }
            
        }

        /// <summary>
        /// 檢查有沒有草稿(IsBuffer = 1)，有的話就帶回資料，沒有的話就不帶
        /// </summary>
        /// <returns></returns>
        public string AuditCheckBuffer(string IPRSN)
        { 
            var RepairAuIn = db.RepairAuditInfo.Where(x => x.IPRSN == IPRSN).Where(x => x.IsBuffer == true).FirstOrDefault();
            if (RepairAuIn != null)
            {
                //有草稿要回傳
                //要在確認回傳格式
                AllRepairAudit ReAu = new AllRepairAudit()
                { 
                    
                };
                return "";
            }
            else
            {
                //沒有草稿
                return "";
            }
        }

        public string GetSupplementEditData(string IPRSN) //取得"補件"下方編輯區資料，提供給前端做顯示
        {
            var AUID = db.RepairAuditInfo.Where(x => x.IPRSN == IPRSN).FirstOrDefault();
            var AUName = db.AspNetUsers.Where(x => x.UserName == AUID.AuditUserID).Select(x => x.MyName).FirstOrDefault();
            
            
            return "";
        }
    }
}