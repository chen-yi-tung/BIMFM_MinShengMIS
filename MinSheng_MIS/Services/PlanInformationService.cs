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
    public class PlanInformationService : IDisposable
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 巡檢資訊管理 Services

        #region 巡檢總計畫完成狀態
        public JArray ChartInspectionCompleteState(DateTime StartDate, DateTime EndDate)
        {
            JArray ChartInspectionCompleteState = new JArray();
            var inspectionplan = db.InspectionPlan.Where(x => x.PlanDate >= StartDate && x.PlanDate < EndDate).ToList();
            var InspectionPlanStatedic = Surface.InspectionPlanState();
            foreach (var item in InspectionPlanStatedic)
            {
                JObject jo = new JObject();
                jo.Add("label", item.Value);
                jo.Add("value", Convert.ToInt32(inspectionplan.Where(x => x.PlanState == item.Key).Count()));
                ChartInspectionCompleteState.Add(jo);
            } 
            return ChartInspectionCompleteState;
        }
        #endregion

        #region 設備維修及保養統計
        public JArray ChartInspectionEquipmentState(DateTime StartDate, DateTime EndDate)
        {
            JArray ChartInspectionEquipmentState = new JArray();
            var RepairEquipments = (from x1 in db.EquipmentReportForm
                                    where x1.RepairTime >= StartDate && x1.RepairTime < EndDate && x1.ReportState == "4"
                                    select new { x1.ESN })
                                  .Distinct().ToList(); //該檢索時間段所維修過的設備
            var MaintainEquipments = (from x1 in db.Equipment_MaintenanceForm
                                      where x1.ReportTime >= StartDate && x1.ReportTime < EndDate && x1.Status == "4"
                                      select new { x1.ESN }).Distinct().ToList(); //該檢索時間段所保養的設備
            var intersection = RepairEquipments.Intersect(MaintainEquipments); //找出在該檢索時間段有做保養及維修之設備
            JObject rm = new JObject { { "label", "保養" }, { "value", MaintainEquipments.Count() - intersection.Count() } };
            ChartInspectionEquipmentState.Add(rm);
            JObject r = new JObject { { "label", "維修" }, { "value", RepairEquipments.Count() - intersection.Count() } };
            ChartInspectionEquipmentState.Add(r);
            JObject m = new JObject { { "label", "保養+維修" }, { "value", intersection.Count() } };
            ChartInspectionEquipmentState.Add(m); 

            return ChartInspectionEquipmentState;
        }
        #endregion

        #region 巡檢人員表格
        public JArray InspectionMembers(DateTime StartDate, DateTime EndDate)
        {
            JArray InspectionMembers = new JArray();
            var inspectionplanUserNameList = (from x1 in db.InspectionPlan
                                             where x1.PlanDate >= StartDate && x1.PlanDate < EndDate
                                             join x2 in db.InspectionPlan_Time on x1.IPSN equals x2.IPSN
                                             join x3 in db.InspectionPlan_Member on x2.IPTSN equals x3.IPTSN
                                             join x4 in db.AspNetUsers on x3.UserID equals x4.UserName
                                             select new { x4.UserName, x4.MyName}).ToList().Union(from x1 in db.Equipment_MaintenanceForm
                                                                                         where x1.NextMaintainDate >= StartDate && x1.NextMaintainDate < EndDate
                                                                                         join x2 in db.Equipment_MaintenanceFormMember on x1.EMFSN equals x2.EMFSN
                                                                                         join x3 in db.AspNetUsers on x2.Maintainer equals x3.UserName
                                                                                         select new { x3.UserName, x3.MyName }).ToList().Union(from x1 in db.EquipmentReportForm
                                                                                                                                      where x1.ReportTime >= StartDate && x1.ReportTime < EndDate
                                                                                                                                      join x2 in db.Equipment_ReportFormMember on x1.RSN equals x2.RSN
                                                                                                                                      join x3 in db.AspNetUsers on x2.RepairUserName equals x3.UserName
                                                                                                                                      select new { x3.UserName, x3.MyName }).ToList();
            foreach (var member in inspectionplanUserNameList)
            {
                JObject jo = new JObject();
                jo.Add("MyName", member.MyName);//人員姓名
                var planlist = (from x1 in db.InspectionPlan
                               where x1.PlanDate >= StartDate && x1.PlanDate < EndDate
                               join x2 in db.InspectionPlan_Time on x1.IPSN equals x2.IPSN
                               join x3 in db.InspectionPlan_Member on x2.IPTSN equals x3.IPTSN
                               where x3.UserID == member.UserName
                               select new { x2.IPTSN, x2.InspectionState }).ToList();
                var maintainlist = (from x1 in db.Equipment_MaintenanceForm
                                   where x1.NextMaintainDate >= StartDate && x1.NextMaintainDate < EndDate
                                   join x2 in db.Equipment_MaintenanceFormMember on x1.EMFSN equals x2.EMFSN
                                   where x2.Maintainer == member.UserName
                                   select new { x1.EMFSN, x1.Status }).ToList();
                var repairlist = (from x1 in db.EquipmentReportForm
                                 where x1.ReportTime >= StartDate && x1.ReportTime < EndDate
                                 join x2 in db.Equipment_ReportFormMember on x1.RSN equals x2.RSN
                                 where x2.RepairUserName == member.UserName
                                 select new { x1.RSN, x1.ReportState }).ToList();
                int PlanNum = planlist.Count();//巡檢總數
                int MaintainNum = maintainlist.Count();//保養總數
                int RepairNum = repairlist.Count();//維修總數
                int FinishPlanNum = planlist.Where(x => x.InspectionState == "3").Count();//巡檢完成總數
                int FinishMaintainNum = maintainlist.Where(x => x.Status == "4").Count();//保養完成總數
                int FinishRepairNum = repairlist.Where(x => x.ReportState == "4").Count();//維修完成總數

                jo.Add("PlanNum", PlanNum);//巡檢總數
                jo.Add("PlanCompleteNum", FinishPlanNum);//巡檢完成數
                float PlanCompletionRate = (PlanNum != 0) ? (float)(FinishPlanNum) / PlanNum : 0;
                jo.Add("PlanCompletionRate", float.IsNaN(PlanCompletionRate) ? 0 : PlanCompletionRate);//巡檢完成率
                jo.Add("MaintainNum", MaintainNum);//保養總數
                jo.Add("MaintainCompleteNum", FinishMaintainNum);//保養完成數
                float maintainCompletionRate = (MaintainNum != 0) ? (float)(FinishMaintainNum) / MaintainNum : 0;
                jo.Add("MaintainCompletionRate", float.IsNaN(maintainCompletionRate) ? 0 : maintainCompletionRate);//保養完成率
                jo.Add("RepairNum", RepairNum);//維修總數
                jo.Add("RepairCompleteNum", FinishRepairNum);//維修完成數
                float RepairCompletionRate = (RepairNum != 0) ? (float)(FinishRepairNum) / RepairNum : 0;
                jo.Add("RepairCompletionRate", float.IsNaN(RepairCompletionRate) ? 0 : RepairCompletionRate);//維修完成率

                InspectionMembers.Add(jo);
            } 
            return InspectionMembers;
        }
        #endregion

        #region 緊急事件等級占比
        public JArray ChartInspectionAberrantLevel(DateTime StartDate, DateTime EndDate)
        {
            JArray ChartInspectionAberrantLevel = new JArray();
            var MessageList = db.WarningMessage.Where(x => x.TimeOfOccurrence >= StartDate && x.TimeOfOccurrence < EndDate).ToList();
            var WMTypeDic = Surface.WMType();
            foreach (var item in WMTypeDic)
            {
                JObject jo = new JObject();
                jo.Add("label", item.Value);
                jo.Add("value", MessageList.Where(x => x.WMType == item.Key).Count());
                ChartInspectionAberrantLevel.Add(jo);
            }
            return ChartInspectionAberrantLevel;
        }
        #endregion

        #region 緊急事件處理狀況
        public JArray ChartInspectionAberrantResolve(DateTime StartDate, DateTime EndDate)
        {
            JArray ChartInspectionAberrantResolve = new JArray();
            var MessageList = db.WarningMessage.Where(x => x.TimeOfOccurrence >= StartDate && x.TimeOfOccurrence < EndDate).ToList();
            var WMStateDic = Surface.WMState();
            foreach (var item in WMStateDic)
            {
                JObject jo = new JObject();
                jo.Add("label", item.Value);
                jo.Add("value", MessageList.Where(x => x.WMState == item.Key).Count());
                ChartInspectionAberrantResolve.Add(jo);
            }
            return ChartInspectionAberrantResolve;
        }
        #endregion

        #region 設備保養及維修進度統計
        public JArray ChartEquipmentProgressStatistics(DateTime StartDate, DateTime EndDate)
        {
            JArray ChartEquipmentProgressStatistics = new JArray();
            var MaintainStatuseDic = Surface.MaintainStatus();
            var MaintainList = (from x1 in db.Equipment_MaintenanceForm
                               where x1.NextMaintainDate >= StartDate && x1.NextMaintainDate < EndDate
                               select new { x1.EMFSN, x1.Status }).ToList(); //該檢索時間段的所有設備保養單紀錄
            var RepairList = (from x1 in db.EquipmentReportForm
                             where x1.ReportTime >= StartDate && x1.ReportTime < EndDate
                             select new { x1.RSN, x1.ReportState }).ToList(); //該檢索時間段的所有設備維修紀錄

            foreach (var item in MaintainStatuseDic)
            {
                JObject jo = new JObject();
                JObject mr = new JObject();
                mr.Add("Maintain", MaintainList.Where(x => x.Status == item.Key).Count());
                mr.Add("Repair", RepairList.Where(x => x.ReportState == item.Key).Count());
                jo.Add("label", item.Value);
                jo.Add("value", mr);
                ChartEquipmentProgressStatistics.Add(jo);
            }
            return ChartEquipmentProgressStatistics;
        }
        #endregion

        #region 設備故障等級分布
        public JArray ChartEquipmentLevelRate(DateTime StartDate, DateTime EndDate)
        {
            JArray ChartEquipmentLevelRate = new JArray();
            var reportList = db.EquipmentReportForm.Where(x => x.ReportTime >= StartDate && x.ReportTime < EndDate).ToList();
            var ReportLevelDic = Surface.ReportLevel();
            foreach (var item in ReportLevelDic)
            {
                JObject jo = new JObject();
                jo.Add("label", item.Value);
                jo.Add("value", reportList.Where(x => x.ReportLevel == item.Key).Count());
                ChartEquipmentLevelRate.Add(jo);
            }
            return ChartEquipmentLevelRate;
        }
        #endregion

        #region 設備故障類型占比
        public JArray ChartEquipmentTypeRate(DateTime StartDate, DateTime EndDate)
        {
            JArray ChartEquipmentTypeRate = new JArray();
            //統計該區間設備故障類型占比
            var RepairEquipmentType = (from x1 in db.EquipmentReportForm
                                      where x1.ReportTime >= StartDate && x1.ReportTime < EndDate
                                      join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                                      group x1 by new { x2.EName } into grouped
                                      orderby grouped.Count() descending
                                      select new { Type = grouped.Key, Count = grouped.Count() }).ToList();
            foreach (var item in RepairEquipmentType)
            {
                JObject jo = new JObject();
                jo.Add("label", item.Type.EName);
                jo.Add("value", item.Count);
                ChartEquipmentTypeRate.Add(jo);
            }
            return ChartEquipmentTypeRate;
        }
        #endregion

        #endregion

        #region 巡檢即時位置 Services

        #region 設備運轉狀態
        public JArray EquipmentOperatingState(string FSN)
        {
            JArray EquipmentOperatingState = new JArray();
            return EquipmentOperatingState;
        }
        #endregion

        #region 環境資訊
        public JArray EnvironmentInfo(string FSN)
        {
            JArray EnvironmentInfo = new JArray();
            return EnvironmentInfo;
        }
        #endregion

        #region 空間人員即時位置
        public JObject InspectionCurrentPos(string FSN)
        {
            JObject jo = new JObject();
            jo["current"] = new JArray();
            jo["another"] = new JArray();
            return jo;
        }
        #endregion

        #endregion

        #region Old Services

        #region 巡檢計畫列表
        /*public JArray GetInspection_Plan_List(DateTime StartDate, DateTime EndDate)
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
        }*/
        #endregion

        #region 當前巡檢狀況
        /*public JObject GetInspection_Member(DateTime StartDate, DateTime EndDate)
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
        }*/
        #endregion

        #region 當前巡檢人員列表
        /*public JArray GetPlan_People_List(string IPSN)
        {
            JArray Plan_People_List = new JArray();
            DateTime StartDate = DateTime.Today;
            DateTime EndDate = DateTime.Today.AddDays(1);
            //找出今日的所有計畫人員
            var inspectorlist = from x1 in db.InspectionPlan
                                where x1.PlanState == "2" && x1.PlanDate >= StartDate && x1.PlanDate < EndDate
                                join x2 in db.InspectionPlanMember on x1.IPSN equals x2.IPSN
                                join x3 in db.AspNetUsers on x2.UserID equals x3.UserName
                                select new { x1.IPSN, x2.PMSN, x3.MyName };
            if (!string.IsNullOrEmpty(IPSN)) //若前台有篩選計畫，則只列出該計畫編號之人員
            {
                inspectorlist = inspectorlist.Where(x => x.IPSN == IPSN);
            }
            foreach (var member in inspectorlist)
            {
                var currentdata = db.InspectionTrack.Where(x => x.PMSN == member.PMSN).OrderByDescending(x => x.ITSN).FirstOrDefault();
                JObject jo = new JObject{
                    {"PMSN", member.PMSN}, //巡檢計畫人員編號
                    {"IPSN", "Pt" + member.IPSN.Substring(Math.Max(0, member.IPSN.Length - 2))}, //計畫編號(縮寫)
                    {"MyName", member.MyName}, //巡檢人員名稱
                    {"Location", db.Floor_Info.Find(currentdata.FSN).AreaInfo.Area.ToString() + " " + db.Floor_Info.Find(currentdata.FSN).FloorName.ToString()}, //巡檢人員所在位置
                    { "Heartbeat", currentdata.Heartbeat} //巡檢人員心率
                };
                Plan_People_List.Add(jo);
            }
            return Plan_People_List;
        }*/

        #endregion

        #endregion

        public void Dispose()
        {
            db.Dispose();
        }
    }
}