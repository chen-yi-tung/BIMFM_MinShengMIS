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
            var planList = db.InspectionPlan.Where(x => x.PlanDate < endDate && x.PlanState == "1").ToList();
            foreach(var plan in planList)
            {
                //巡檢計畫狀態改為巡檢未完成
                var inspectionplan = db.InspectionPlan.Find(plan.IPSN);
                inspectionplan.PlanState = "4"; //巡檢未完成
                db.InspectionPlan.AddOrUpdate(inspectionplan);
                //保養單恢復為上一個狀態
                if(plan.MaintainAmount != 0)
                {
                    var maintainList = db.InspectionPlanMaintain.Where(x => x.IPSN == plan.IPSN).ToList();
                    foreach(var maintain in maintainList)
                    {
                        var check1 = db.InspectionPlanMaintain.Where(x => x.IPSN != plan.IPSN && x.EMFISN == maintain.EMFISN).OrderByDescending(x => x.MaintainDate ).ToList();
                        //待派工判斷 沒有派去其他計畫過
                        if(check1.Count == 0)
                        {
                            var maintainChangeState = db.EquipmentMaintainFormItem.Find(maintain.EMFISN);
                            maintainChangeState.FormItemState = "1";
                            db.EquipmentMaintainFormItem.AddOrUpdate(maintainChangeState);
                            db.SaveChanges();
                        }
                        else
                        {
                            //未完成判斷
                            if (check1[0].MaintainStateOfFilling == "2") // 未完成
                            {
                                var maintainChangeState = db.EquipmentMaintainFormItem.Find(maintain.EMFISN);
                                maintainChangeState.FormItemState = "5";
                                db.EquipmentMaintainFormItem.AddOrUpdate(maintainChangeState);
                                db.SaveChanges();
                            }
                            //審核未過判斷
                            else
                            {
                                var maintainChangeState = db.EquipmentMaintainFormItem.Find(maintain.EMFISN);
                                maintainChangeState.FormItemState = "8";
                                db.EquipmentMaintainFormItem.AddOrUpdate(maintainChangeState);
                                db.SaveChanges();
                            }
                        }
                        
                    }
                }
                //報修單恢復為上一個狀態
                if(plan.RepairAmount != 0)
                {

                }
            }
        }
    }
}