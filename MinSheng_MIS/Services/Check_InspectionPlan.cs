using MinSheng_MIS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Razor.Generator;

namespace MinSheng_MIS.Services
{
    public class Check_InspectionPlan
    {
        public void CheckInspectionPlan()
        {
            Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
            DateTime endDate = DateTime.Today.AddDays(1);
            var planList = db.InspectionPlan.Where(x => x.PlanDate < endDate && x.PlanState == "1").ToList(); //狀態為待執行但計畫執行時間已過期
            foreach(var plan in planList)
            {
                //巡檢計畫狀態改為巡檢未完成
                var inspectionplan = db.InspectionPlan.Find(plan.IPSN);
                inspectionplan.PlanState = "4"; //巡檢未完成
                db.InspectionPlan.AddOrUpdate(inspectionplan);
                db.SaveChanges();
                //保養單恢復為上一個狀態
                if(plan.MaintainAmount != 0)
                {
                    var maintainList = db.InspectionPlanMaintain.Where(x => x.IPSN == plan.IPSN).ToList();
                    foreach(var maintain in maintainList)
                    {
                        var check = db.InspectionPlanMaintain.Where(x => x.IPSN != plan.IPSN && x.EMFISN == maintain.EMFISN).OrderByDescending(x => x.MaintainDate).ToList();
                        var maintainChangeState = db.EquipmentMaintainFormItem.Find(maintain.EMFISN);
                        //待派工判斷 沒有派去其他計畫過
                        if (check.Count == 0)
                        {
                            maintainChangeState.FormItemState = "1";//待派工
                        }
                        else
                        {
                            var check1 = db.InspectionPlanMaintain.Where(x => x.IPSN != plan.IPSN && x.EMFISN == maintain.EMFISN).OrderByDescending(x => x.MaintainDate).FirstOrDefault();
                            if(check1 != null)
                            {
                                //未完成判斷
                                if (check1.MaintainStateOfFilling == "2")
                                {
                                    maintainChangeState.FormItemState = "5";//未完成
                                }
                                //審核未過判斷
                                else
                                {
                                    maintainChangeState.FormItemState = "8";//審核未過
                                }
                            }
                        }
                        db.EquipmentMaintainFormItem.AddOrUpdate(maintainChangeState);
                        db.SaveChanges();
                    }
                }
                //報修單恢復為上一個狀態
                if(plan.RepairAmount != 0)
                {
                    var repairList = db.InspectionPlanRepair.Where(x => x.IPSN == plan.IPSN).ToList();
                    foreach (var repair in repairList)
                    {
                        var check = db.InspectionPlanRepair.Where(x => x.IPSN != plan.IPSN && x.RSN == repair.RSN).OrderByDescending(x => x.RepairDate).ToList();
                        var reportChangeState = db.EquipmentReportForm.Find(repair.RSN);
                        //待派工判斷 沒有派去其他計畫過
                        if (check.Count() == 0)
                        {
                            reportChangeState.ReportState = "1";//待派工
                        }
                        else
                        {
                            var check1 = db.InspectionPlanRepair.Where(x => x.IPSN != plan.IPSN && x.RSN == repair.RSN).OrderByDescending(x => x.RepairDate).FirstOrDefault();
                            if(check1 != null)
                            {
                                //未完成判斷
                                if (check1.RepairStateOfFilling == "2")
                                {
                                    reportChangeState.ReportState = "5";//未完成
                                }
                                else
                                {
                                    reportChangeState.ReportState = "8";//審核未過
                                }
                            }
                        }
                        db.EquipmentReportForm.AddOrUpdate(reportChangeState);
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}