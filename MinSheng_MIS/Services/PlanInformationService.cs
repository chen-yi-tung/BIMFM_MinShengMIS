using MinSheng_MIS.Models;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Services
{
    public class PlanInformationService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 巡檢計畫完成狀態
        public JArray GetInspection_Complete_State(DateTime StartDate, DateTime EndDate) //巡檢計畫完成狀態
        {
            JArray Inspection_Complete_State = new JArray();
            var inspectionplan = db.InspectionPlan.Where(x => x.PlanDate >= StartDate && x.PlanDate < EndDate);
            var InspectionPlanStatedic = Surface.InspectionPlanState();
            foreach (var item in InspectionPlanStatedic)
            {
                if (item.Key != "5") //排除計畫狀態為停用的
                {
                    JObject jo = new JObject();
                    jo.Add("label", item.Value);
                    jo.Add("value", Convert.ToInt32(inspectionplan.Where(x => x.PlanState == item.Key).Count()));
                    Inspection_Complete_State.Add(jo);
                }
            }
            return Inspection_Complete_State;
        }
        #endregion

        #region 巡檢設備統計
        public JArray GetInspection_Equipment_State(DateTime StartDate, DateTime EndDate) //巡檢設備統計
        {
            JArray Inspection_Equipment_State = new JArray();
            var inspectionplan = db.InspectionPlan.Where(x => x.PlanDate >= StartDate && x.PlanDate < EndDate);
            var RepairEquipments = (from x1 in inspectionplan
                                    join x2 in db.InspectionPlanRepair on x1.IPSN equals x2.IPSN
                                    join x3 in db.EquipmentReportForm on x2.RSN equals x3.RSN
                                    select new { x3.ESN })
                                  .Distinct(); //該檢索時間段所維修過的設備
            var MaintainEquipments = (from x1 in inspectionplan
                                      join x2 in db.InspectionPlanMaintain on x1.IPSN equals x2.IPSN
                                      join x3 in db.EquipmentMaintainFormItem on x2.EMFISN equals x3.EMFISN
                                      join x4 in db.EquipmentMaintainItem on x3.EMISN equals x4.EMISN
                                      select new { x4.ESN }).Distinct(); //該檢索時間段所保養的設備
            var intersection = RepairEquipments.Intersect(MaintainEquipments); //找出在該檢索時間段有做保養及維修之設備
            JObject rm = new JObject { { "label", "保養" }, { "value", MaintainEquipments.Count() - intersection.Count() } };
            Inspection_Equipment_State.Add(rm);
            JObject r = new JObject { { "label", "維修" }, { "value", RepairEquipments.Count() - intersection.Count() } };
            Inspection_Equipment_State.Add(r);
            JObject m = new JObject { { "label", "保養+維修" }, { "value", intersection.Count() } };
            Inspection_Equipment_State.Add(m);

            return Inspection_Equipment_State;
        }
        #endregion

        #region 巡檢人員清單
        public JArray GetInspection_All_Members(DateTime StartDate, DateTime EndDate) //巡檢人員清單
        {
            JArray Inspection_All_Members = new JArray();
            var inspectionplan = db.InspectionPlan.Where(x => x.PlanDate >= StartDate && x.PlanDate < EndDate);
            var IPSNList = inspectionplan.Select(x => x.IPSN).ToList();
            var UserNameList = db.InspectionPlanMember.Where(x => IPSNList.Contains(x.IPSN)).Select(x => x.UserID).Distinct().ToList();
            foreach (var planmember in UserNameList)
            {
                JObject jo = new JObject();
                jo.Add("MyName", db.AspNetUsers.Where(x => x.UserName == planmember).FirstOrDefault().MyName.ToString());//人員姓名
                var memberIPSN = db.InspectionPlanMember.Where(x => x.UserID == planmember && IPSNList.Contains(x.IPSN)).Select(x => x.IPSN).ToList();

                int PlanNum = memberIPSN.Count();//巡檢總數
                int MaintainNum = db.InspectionPlanMaintain.Where(x => memberIPSN.Contains(x.IPSN)).Count();//保養總數
                int RepairNum = db.InspectionPlanRepair.Where(x => memberIPSN.Contains(x.IPSN)).Count();//維修總數
                int FinishPlanNum = db.InspectionPlan.Where(x => memberIPSN.Contains(x.IPSN) && x.PlanState == "3").Count();//巡檢完成總數
                int FinishMaintainNum = db.InspectionPlanMaintain.Where(x => memberIPSN.Contains(x.IPSN) && x.MaintainState == "6").Count();//保養完成總數
                int FinishRepairNum = db.InspectionPlanRepair.Where(x => memberIPSN.Contains(x.IPSN) && x.RepairState == "6").Count();//維修完成總數

                jo.Add("PlanNum", PlanNum);//巡檢總數
                jo.Add("MaintainNum", MaintainNum);//保養總數
                jo.Add("RepairNum", RepairNum);//維修總數
                jo.Add("CompleteNum", FinishPlanNum + FinishMaintainNum + FinishRepairNum);//巡檢完成數
                jo.Add("CompletionRate", (float)(FinishPlanNum + FinishMaintainNum + FinishRepairNum) / (PlanNum + MaintainNum + RepairNum));//完成率
                Inspection_All_Members.Add(jo);
            }
            return Inspection_All_Members;
        }
        #endregion

        #region 緊急事件等級占比
        public JArray GetInspection_Aberrant_Level(DateTime StartDate, DateTime EndDate) //緊急事件等級占比
        {
            JArray Inspection_Aberrant_Level = new JArray();
            var MessageList = db.WarningMessage.Where(x => x.TimeOfOccurrence >= StartDate && x.TimeOfOccurrence < EndDate);
            var WMTypeDic = Surface.WMType();
            foreach (var item in WMTypeDic)
            {
                JObject jo = new JObject();
                jo.Add("label", item.Value);
                jo.Add("value", MessageList.Where(x => x.WMType == item.Key).Count());
                Inspection_Aberrant_Level.Add(jo);
            }
            return Inspection_Aberrant_Level;
        }
        #endregion

        #region 緊急事件處理狀況
        public JArray GetInspection_Aberrant_Resolve(DateTime StartDate, DateTime EndDate) //緊急事件處理狀況
        {
            JArray Inspection_Aberrant_Resolve = new JArray();
            var MessageList = db.WarningMessage.Where(x => x.TimeOfOccurrence >= StartDate && x.TimeOfOccurrence < EndDate);
            var WMStateDic = Surface.WMState();
            foreach (var item in WMStateDic)
            {
                JObject jo = new JObject();
                jo.Add("label", item.Value);
                jo.Add("value", MessageList.Where(x => x.WMState == item.Key).Count());
                Inspection_Aberrant_Resolve.Add(jo);
            }
            return Inspection_Aberrant_Resolve;
        }
        #endregion

        #region 設備保養及維修進度統計
        public JArray GetEquipment_Maintain_And_Repair_Statistics(DateTime StartDate, DateTime EndDate) //設備保養及維修進度統計
        {
            JArray Equipment_Maintain_And_Repair_Statistics = new JArray();
            var inspectionplan = db.InspectionPlan.Where(x => x.PlanDate >= StartDate && x.PlanDate < EndDate);
            var InspectionPlanRepairStateDic = Surface.InspectionPlanRepairState();
            var MaintainList = from x1 in inspectionplan
                               join x2 in db.InspectionPlanMaintain on x1.IPSN equals x2.IPSN
                               select new { x2.IPMSN, x2.MaintainState }; //該檢索時間段的所有設備保養單紀錄
            var RepairList = from x1 in inspectionplan
                             join x2 in db.InspectionPlanRepair on x1.IPSN equals x2.IPSN
                             select new { x2.IPRSN, x2.RepairState }; //該檢索時間段的所有設備維修紀錄

            foreach (var item in InspectionPlanRepairStateDic)
            {
                JObject jo = new JObject();
                JObject mr = new JObject();
                mr.Add("Maintain", MaintainList.Where(x => x.MaintainState == item.Key).Count());
                mr.Add("Repair", RepairList.Where(x => x.RepairState == item.Key).Count());
                jo.Add("label", item.Value);
                jo.Add("value", mr);
                Equipment_Maintain_And_Repair_Statistics.Add(jo);
            }
            return Equipment_Maintain_And_Repair_Statistics;
        }
        #endregion

        #region 設備故障等級分布
        public JArray GetEquipment_Level_Rate(DateTime StartDate, DateTime EndDate) //設備故障等級分布
        {
            JArray Equipment_Level_Rate = new JArray();
            var reportList = db.EquipmentReportForm.Where(x => x.Date >= StartDate && x.Date < EndDate);
            var ReportLevelDic = Surface.ReportLevel();
            foreach (var item in ReportLevelDic)
            {
                JObject jo = new JObject();
                jo.Add("label", item.Value);
                jo.Add("value", reportList.Where(x => x.ReportLevel == item.Key).Count());
                Equipment_Level_Rate.Add(jo);
            }
            return Equipment_Level_Rate;
        }
        #endregion

        #region 設備故障類型占比
        public JArray GetEquipment_Type_Rate(DateTime StartDate, DateTime EndDate) //設備故障類型占比
        {
            JArray Equipment_Type_Rate = new JArray();
            //統計該區間設備故障類型占比
            var RepairEquipmentType = from x1 in db.EquipmentReportForm
                                      where x1.Date >= StartDate && x1.Date < EndDate
                                      join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                                      group x1 by new { x2.System, x2.SubSystem } into grouped
                                      orderby grouped.Count() descending
                                      select new { Type = grouped.Key, Count = grouped.Count() };
            foreach (var item in RepairEquipmentType)
            {
                JObject jo = new JObject();
                jo.Add("label", item.Type.System + " " + item.Type.SubSystem);
                jo.Add("value", item.Count);
                Equipment_Type_Rate.Add(jo);
            }
            return Equipment_Type_Rate;
        }
        #endregion

        #region 巡檢計畫列表
        public JArray GetInspection_Plan_List(DateTime StartDate, DateTime EndDate)
        {
            JArray Inspection_Plan_List = new JArray();

            var PlanList = db.InspectionPlan.Where(x => x.PlanDate >= StartDate && x.PlanDate < EndDate).OrderBy(x => x.Shift).ToList();
            var PlanStateDic = Surface.InspectionPlanState();
            var ShiftDic = Surface.Shift();
            foreach (var plan in PlanList)
            {
                JObject jo = new JObject();
                jo.Add("PlanState", PlanStateDic[plan.PlanState]);
                jo.Add("IPName", plan.IPName);
                jo.Add("Shift", ShiftDic[plan.Shift]);
                Inspection_Plan_List.Add(jo);
            }
            return Inspection_Plan_List;
        }
        #endregion

        #region 當前巡檢狀況
        public JObject GetInspection_Member(DateTime StartDate, DateTime EndDate)
        {
            JObject Inspection_Member = new JObject();
            var PlanList = (from x1 in db.InspectionPlan
                        where x1.PlanDate >= StartDate && x1.PlanDate < EndDate && x1.PlanState == "2" //今日巡檢中計畫
                        join x2 in db.InspectionPlanMember on x1.IPSN equals x2.IPSN
                        select x2.PMSN).ToList();
            Inspection_Member.Add("Inspection_Members_All", PlanList.Count()); //目前巡檢人數
            Inspection_Member.Add("Inspection_Members_Notice", db.WarningMessage.Where(x => PlanList.Any(plan => x.PMSN.Contains(plan.ToString())) && x.WMState != "3" && x.WMType == "1").Count()); //偏離路線人數
            Inspection_Member.Add("Inspection_Members_Alert", db.WarningMessage.Where(x => PlanList.Any(plan => x.PMSN.Contains(plan.ToString())) && x.WMState != "3" && x.WMType == "2").Count()); //狀態異常人數

            return Inspection_Member;
        }
        #endregion

        #region 當前巡檢人員列表
        public JArray GetPlan_People_List(string IPSN)
        {
            JArray Plan_People_List = new JArray();
            var inspectorlist = from x1 in db.InspectionPlan
                                where x1.PlanState == "2" && x1.PlanDate >= DateTime.Today && x1.PlanDate < DateTime.Today.AddDays(1)
                                join x2 in db.InspectionPlanMember on x1.IPSN equals x2.IPSN
                                join x3 in db.AspNetUsers on x2.UserID equals x3.UserName
                                select new {x1.IPSN, x2.PMSN, x3.MyName};
            if (string.IsNullOrEmpty(IPSN))
            {
                inspectorlist = inspectorlist.Where(x => x.IPSN == IPSN);
            }
            foreach(var member in inspectorlist)
            {
                var currentdata = db.InspectionTrack.Where(x => x.PMSN == member.PMSN).OrderByDescending(x => x.ITSN).FirstOrDefault();
                JObject jo = new JObject {
                    { "PMSN", member.PMSN},
                    { "IPSN", "Pt" + member.IPSN.Substring(Math.Max(0, member.IPSN.Length - 2))},
                    { "MyName", member.MyName},
                    { "Location", db.Floor_Info.Find(currentdata).AreaInfo.Area.ToString() + " " + db.Floor_Info.Find(currentdata.FSN).FloorName.ToString()},
                    { "Heartbeat", currentdata.Heartbeat}
                };

            }
            return Plan_People_List;
        }
        #endregion
    }
}