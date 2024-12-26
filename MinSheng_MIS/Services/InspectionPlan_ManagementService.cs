using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static MinSheng_MIS.Models.ViewModels.InspectionPlan_ManagementViewModel;

namespace MinSheng_MIS.Services
{
    public class InspectionPlan_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;

        public InspectionPlan_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        #region APP-巡檢工單列表
        public JsonResService<List<PlanInfo>> GetPlanList(string userID,DateTime searchdate)
        {
            #region 變數
            JsonResService<List<PlanInfo>> res = new JsonResService<List<PlanInfo>>();
            var dic_inspectionState = Surface.InspectionState();
            #endregion

            try
            {
                #region 資料檢查
                var data = from x1 in _db.InspectionPlan_Member
                           where x1.UserID == userID
                           join x2 in _db.InspectionPlan_Time on x1.IPTSN equals x2.IPTSN
                           join x3 in _db.InspectionPlan on x2.IPSN equals x3.IPSN
                           where x3.PlanDate == searchdate
                           select new { x3.PlanDate, x2.IPTSN, x2.StartTime, x2.EndTime, x2.InspectionState };
                if (data == null)
                {
                    res.AccessState = ResState.Failed;
                    res.ErrorMessage = "查無此日期之巡檢計畫";
                    return res;
                }
                #endregion

                #region 資料
                List<PlanInfo> datas = new List<PlanInfo>();
                List<string> members = new List<string>();
                foreach (var plan in data)
                {
                    PlanInfo planInfo = new PlanInfo();
                    planInfo.InspectionState = dic_inspectionState[plan.InspectionState];
                    planInfo.IPTSN = plan.IPTSN;
                    planInfo.InspectionTime = plan.PlanDate.ToString("yyyy/MM/dd") + " " + plan.StartTime + "-" + plan.EndTime;
                    var memberlist = from x1 in _db.InspectionPlan_Member
                                  where x1.IPTSN == plan.IPTSN
                                  join x2 in _db.AspNetUsers on x1.UserID equals x2.UserName
                                  select new { x2.MyName};
                    foreach(var member in memberlist)
                    {
                        members.Add(member.MyName);
                    }
                    planInfo.Member= members;
                    datas.Add(planInfo);
                }
                res.Datas = datas;
                #endregion

                res.AccessState = ResState.Success;
                return res;
            }
            catch (Exception ex)
            {
                res.AccessState = ResState.Failed;
                res.ErrorMessage = ex.Message;
                throw;
            }
        }
        #endregion
    }
}